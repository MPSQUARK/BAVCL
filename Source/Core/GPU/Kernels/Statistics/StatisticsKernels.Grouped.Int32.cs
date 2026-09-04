using System;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using BAVCL.Core.Exceptions;
using BAVCL.Core.Kernels;

namespace BAVCL;

public partial class GPU
{	internal Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, ArrayView<int>, int>
		dotReduceIntGroupedKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(dotReduceIntGroupedKernel));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, int>
		sumReduceIntGroupedKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(sumReduceIntGroupedKernel));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, int>
		minMaxReduceIntGroupedKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(minMaxReduceIntGroupedKernel));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<float>, float, int>
		sumSquaredDiffIntGroupedKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(sumSquaredDiffIntGroupedKernel));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, int>
		allNonZeroIntGroupedKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(allNonZeroIntGroupedKernel));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<float>, int>
		varReduceIntGroupedKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(varReduceIntGroupedKernel));

	static void UpdateVarianceAccumInt(ref int count, ref float mean, ref float m2, int value) =>
		UpdateVarianceAccum(ref count, ref mean, ref m2, value);

	static void DotReduceIntGroupedKernel(
		ArrayView<int> left,
		ArrayView<int> right,
		ArrayView<int> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int partial = 0;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			partial += left[i] * right[i];

		int groupSum = GroupExtensions.AllReduce<int, AddInt32>(partial);
		if (Group.IsFirstThread)
			output[Grid.IdxX] = groupSum;
	}

	static void SumReduceIntGroupedKernel(
		ArrayView<int> input,
		ArrayView<int> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int partial = 0;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			partial += input[i];

		int groupSum = GroupExtensions.AllReduce<int, AddInt32>(partial);
		if (Group.IsFirstThread)
			output[Grid.IdxX] = groupSum;
	}

	static void MinMaxReduceIntGroupedKernel(
		ArrayView<int> input,
		ArrayView<int> partialMinMax,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int localMin = int.MaxValue;
		int localMax = int.MinValue;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
		{
			int value = input[i];
			localMin = XMath.Min(localMin, value);
			localMax = XMath.Max(localMax, value);
		}

		int groupMin = GroupExtensions.AllReduce<int, MinInt32>(localMin);
		int groupMax = GroupExtensions.AllReduce<int, MaxInt32>(localMax);

		if (!Group.IsFirstThread)
			return;

		int slot = Grid.IdxX << 1;
		if (slot + 1 >= partialMinMax.Length)
			return;

		partialMinMax[slot] = groupMin;
		partialMinMax[slot + 1] = groupMax;
	}

	static void SumSquaredDiffIntGroupedKernel(
		ArrayView<int> input,
		ArrayView<float> output,
		float mean,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		double sum = 0d;
		double compensation = 0d;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
		{
			float diff = input[i] - mean;
			NeumaierAdd(ref sum, ref compensation, diff * diff);
		}

		float partial = (float)(sum + compensation);
		float groupSum = GroupExtensions.AllReduce<float, AddFloat>(partial);
		if (Group.IsFirstThread)
			output[Grid.IdxX] = groupSum;
	}

	static void AllNonZeroIntGroupedKernel(
		ArrayView<int> input,
		ArrayView<int> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int local = 0;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
		{
			if (input[i] == 0)
				local = 1;
		}

		int groupFlag = GroupExtensions.AllReduce<int, MaxInt32>(local);
		if (Group.IsFirstThread)
			output[Grid.IdxX] = groupFlag;
	}

	internal static void VarReduceIntGroupedSharedKernel(
		ArrayView<int> input,
		ArrayView<float> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int localCount = 0;
		float localMean = 0f;
		float localM2 = 0f;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			UpdateVarianceAccumInt(ref localCount, ref localMean, ref localM2, input[i]);

		var countShared = SharedMemory.Allocate<int>(KernelSharedMemory.GlobalReduceGroupSize);
		var meanShared = SharedMemory.Allocate<float>(KernelSharedMemory.GlobalReduceGroupSize);
		var m2Shared = SharedMemory.Allocate<float>(KernelSharedMemory.GlobalReduceGroupSize);
		countShared[Group.IdxX] = localCount;
		meanShared[Group.IdxX] = localMean;
		m2Shared[Group.IdxX] = localM2;
		Group.Barrier();

		if (Group.IdxX != 0)
			return;

		int accCount = countShared[0];
		float accMean = meanShared[0];
		float accM2 = m2Shared[0];
		for (int thread = 1; thread < Group.DimX; thread++)
			MergeVarianceAccum(ref accCount, ref accMean, ref accM2, countShared[thread], meanShared[thread], m2Shared[thread]);

		WriteVarianceGroupOutput(output, accCount, accMean, accM2);
	}

	internal static void VarReduceIntGroupedWarpTreeKernel(
		ArrayView<int> input,
		ArrayView<float> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int localCount = 0;
		float localMean = 0f;
		float localM2 = 0f;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			UpdateVarianceAccumInt(ref localCount, ref localMean, ref localM2, input[i]);

		ReduceVarianceIntraWarp(ref localCount, ref localMean, ref localM2);

		var countShared = SharedMemory.Allocate<int>(KernelSharedMemory.VarianceWarpLeaderSlots);
		var meanShared = SharedMemory.Allocate<float>(KernelSharedMemory.VarianceWarpLeaderSlots);
		var m2Shared = SharedMemory.Allocate<float>(KernelSharedMemory.VarianceWarpLeaderSlots);
		if (Warp.IsFirstLane)
		{
			countShared[Warp.WarpIdx] = localCount;
			meanShared[Warp.WarpIdx] = localMean;
			m2Shared[Warp.WarpIdx] = localM2;
		}

		Group.Barrier();

		if (Group.IdxX != 0)
			return;

		int accCount = countShared[0];
		float accMean = meanShared[0];
		float accM2 = m2Shared[0];
		for (int warp = 1; warp < KernelSharedMemory.VarianceWarpLeaderSlots; warp++)
			MergeVarianceAccum(ref accCount, ref accMean, ref accM2, countShared[warp], meanShared[warp], m2Shared[warp]);

		WriteVarianceGroupOutput(output, accCount, accMean, accM2);
	}
}
