using System;
using BAVCL.Core.Memory;
using BAVCL.Types;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

/// <summary>GPU compaction selection: broadcast flags, exclusive scan write positions, true count.</summary>
internal static class MaskGpuSelection
{
	internal static TResult Scan<TResult>(
		GPU gpu,
		Mask mask,
		Shape maskShape,
		Shape outputShape,
		Func<ArrayView1D<int, Stride1D.Dense>, ArrayView1D<int, Stride1D.Dense>, int, TResult> consume)
	{
		int elementCount = outputShape.ElementCount;
		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			ArrayView1D<int, Stride1D.Dense> flagsView = flags.View;
			ArrayView1D<int, Stride1D.Dense> writePosView = writePos.View;
			int trueCount = ExclusivePrefixCount(gpu, mask, maskShape, outputShape, flagsView, writePosView);
			return consume(flagsView, writePosView, trueCount);
		}
	}

	static int ExclusivePrefixCount(
		GPU gpu,
		Mask mask,
		Shape maskShape,
		Shape outputShape,
		ArrayView1D<int, Stride1D.Dense> flags,
		ArrayView1D<int, Stride1D.Dense> writePos)
	{
		int elementCount = outputShape.ElementCount;
		gpu.maskBroadcastFlags(
			gpu.DefaultStream,
			elementCount,
			flags,
			mask.GetBuffer().View,
			outputShape.Cols,
			BroadcastStrides.For(maskShape));
		LaunchDenseScan(gpu, gpu.AlgorithmProviders.IntSumExclusive, flags, writePos);
		gpu.Synchronize();

		if (elementCount == 0)
			return 0;

		int[] lastFlag = [0];
		int[] lastWrite = [0];
		flags.SubView(elementCount - 1, 1).CopyToCPU(lastFlag);
		writePos.SubView(elementCount - 1, 1).CopyToCPU(lastWrite);
		return lastWrite[0] + lastFlag[0];
	}

	static void LaunchDenseScan<T>(
		GPU gpu,
		Scan<T, Stride1D.Dense, Stride1D.Dense> scan,
		ArrayView1D<T, Stride1D.Dense> input,
		ArrayView1D<T, Stride1D.Dense> output)
		where T : unmanaged
	{
		if (input.Length == 0)
			return;

		long tempElements = gpu.accelerator.ComputeScanTempStorageSize<T>(input.Length);
		using BufferEntity<int> temp = BufferPools.For(gpu).Int.Rent((int)tempElements);
		using (GpuScope.Begin(temp.PinTarget))
			scan(gpu.DefaultStream, input, output, temp.View);
	}
}
