using System;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using BAVCL.Core.Exceptions;
using BAVCL.Core.Kernels;

namespace BAVCL;

public partial class GPU
{
	internal Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<float>, ArrayView<float>, int>
		dotReduceFloatGroupedCompensated
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(dotReduceFloatGroupedCompensated));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<float>, int>
		minMaxReduceFloatGrouped
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(minMaxReduceFloatGrouped));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<float>, int>
		sumReduceFloatGroupedCompensated
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(sumReduceFloatGroupedCompensated));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<int>, int>
		allNonZeroFloatGrouped
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(allNonZeroFloatGrouped));

	internal Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<float>, int>
		varReduceFloatGrouped
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(varReduceFloatGrouped));

	// Neumaier EC; see citations.md [9].
	static void NeumaierAdd(ref double sum, ref double compensation, float input)
	{
		double value = input;
		double t = sum + value;
		if (XMath.Abs(sum) >= XMath.Abs(value))
			compensation += (sum - t) + value;
		else
			compensation += (value - t) + sum;
		sum = t;
	}

	// Welford update; see citations.md [11].
	static void UpdateVarianceAccum(ref int count, ref float mean, ref float m2, float value)
	{
		count++;
		float delta = value - mean;
		mean += delta / count;
		m2 += delta * (value - mean);
	}

	// CGL parallel merge; see citations.md [2].
	static void MergeVarianceAccum(ref int countA, ref float meanA, ref float m2A, int countB, float meanB, float m2B)
	{
		if (countB == 0)
			return;

		if (countA == 0)
		{
			countA = countB;
			meanA = meanB;
			m2A = m2B;
			return;
		}

		int count = countA + countB;
		float delta = meanB - meanA;
		m2A = m2A + m2B + delta * delta * countA * countB / count;
		meanA += delta * countB / count;
		countA = count;
	}

	static void DotReduceFloatGroupedCompensated_Kern(
		ArrayView<float> left,
		ArrayView<float> right,
		ArrayView<float> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		double sum = 0d;
		double compensation = 0d;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			NeumaierAdd(ref sum, ref compensation, left[i] * right[i]);

		float partial = (float)(sum + compensation);
		float groupSum = GroupExtensions.AllReduce<float, AddFloat>(partial);
		if (Group.IsFirstThread)
			output[Grid.IdxX] = groupSum;
	}

	static void MinMaxReduceFloatGrouped_Kern(
		ArrayView<float> input,
		ArrayView<float> partialMinMax,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		float localMin = float.PositiveInfinity;
		float localMax = float.NegativeInfinity;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
		{
			float value = input[i];
			localMin = XMath.Min(localMin, value);
			localMax = XMath.Max(localMax, value);
		}

		float groupMin = GroupExtensions.AllReduce<float, MinFloat>(localMin);
		float groupMax = GroupExtensions.AllReduce<float, MaxFloat>(localMax);

		if (!Group.IsFirstThread)
			return;

		int slot = Grid.IdxX << 1;
		if (slot + 1 >= partialMinMax.Length)
			return;

		partialMinMax[slot] = groupMin;
		partialMinMax[slot + 1] = groupMax;
	}

	static void SumReduceFloatGroupedCompensated_Kern(
		ArrayView<float> input,
		ArrayView<float> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		double sum = 0d;
		double compensation = 0d;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			NeumaierAdd(ref sum, ref compensation, input[i]);

		float partial = (float)(sum + compensation);
		float groupSum = GroupExtensions.AllReduce<float, AddFloat>(partial);
		if (Group.IsFirstThread)
			output[Grid.IdxX] = groupSum;
	}

	static void AllNonZeroFloatGrouped_Kern(
		ArrayView<float> input,
		ArrayView<int> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int local = 0;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
		{
			if (input[i] == 0f)
				local = 1;
		}

		int groupFlag = GroupExtensions.AllReduce<int, MaxInt32>(local);
		if (Group.IsFirstThread)
			output[Grid.IdxX] = groupFlag;
	}

	static void ReduceVarianceIntraWarp(ref int count, ref float mean, ref float m2)
	{
		for (int offset = Warp.WarpSize / 2; offset > 0; offset >>= 1)
		{
			int partnerCount = Warp.ShuffleDown(count, offset);
			float partnerMean = Warp.ShuffleDown(mean, offset);
			float partnerM2 = Warp.ShuffleDown(m2, offset);
			if (Warp.LaneIdx < offset)
				MergeVarianceAccum(ref count, ref mean, ref m2, partnerCount, partnerMean, partnerM2);
		}
	}

	static void WriteVarianceGroupOutput(ArrayView<float> output, int count, float mean, float m2)
	{
		int slot = Grid.IdxX * 3;
		if (slot + 2 >= output.Length)
			return;

		output[slot] = count;
		output[slot + 1] = mean;
		output[slot + 2] = m2;
	}

	internal static void VarReduceFloatGroupedShared_Kern(
		ArrayView<float> input,
		ArrayView<float> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int localCount = 0;
		float localMean = 0f;
		float localM2 = 0f;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			UpdateVarianceAccum(ref localCount, ref localMean, ref localM2, input[i]);

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

	internal static void VarReduceFloatGroupedWarpTree_Kern(
		ArrayView<float> input,
		ArrayView<float> output,
		int length)
	{
		int stride = Grid.DimX * Group.DimX;
		int localCount = 0;
		float localMean = 0f;
		float localM2 = 0f;
		for (int i = Grid.IdxX * Group.DimX + Group.IdxX; i < length; i += stride)
			UpdateVarianceAccum(ref localCount, ref localMean, ref localM2, input[i]);

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
