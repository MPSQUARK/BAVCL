using BAVCL.Core.Memory;
using BAVCL.Types;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

/// <summary>GPU compaction selection: broadcast flags, exclusive scan write positions, true count.</summary>
internal static class MaskGpuSelection
{
	internal static SelectionLease RunExclusiveScan(GPU gpu, Mask mask, Shape maskShape, Shape outputShape)
	{
		int elementCount = outputShape.ElementCount;
		BufferEntity<int>? flags = null;
		BufferEntity<int>? writePos = null;

		try
		{
			flags = BufferPools.For(gpu).Int.Rent(elementCount);
			writePos = BufferPools.For(gpu).Int.Rent(elementCount);

			int trueCount;
			using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
			{
				ArrayView1D<int, Stride1D.Dense> flagsView = flags.View;
				ArrayView1D<int, Stride1D.Dense> writePosView = writePos.View;

				gpu.maskBroadcastFlags(
					gpu.DefaultStream,
					elementCount,
					flagsView,
					mask.GetBuffer().View,
					outputShape.Cols,
					BroadcastStrides.For(maskShape));

				Scan<int, Stride1D.Dense, Stride1D.Dense> scan = gpu.AlgorithmProviders.IntSumExclusive;
				LaunchDenseScan(gpu, scan, flagsView, writePosView);
				gpu.Synchronize();

				trueCount = 0;
				if (elementCount > 0)
				{
					int last = elementCount - 1;
					int[] lastFlag = [0];
					int[] lastWrite = [0];
					flagsView.SubView(last, 1).CopyToCPU(lastFlag);
					writePosView.SubView(last, 1).CopyToCPU(lastWrite);
					trueCount = lastWrite[0] + lastFlag[0];
				}
			}

			return new SelectionLease(flags, writePos, elementCount, trueCount);
		}
		catch
		{
			writePos?.Dispose();
			flags?.Dispose();
			throw;
		}
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
