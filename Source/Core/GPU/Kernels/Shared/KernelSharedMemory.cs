using System;
using ILGPU.Runtime;

namespace BAVCL.Core.Kernels;

/// <summary>
/// Static shared-memory budgets for explicitly grouped kernels.
/// Not tracked by <see cref="BAVCL.Core.Memory.LRU"/> (device global memory only).
/// </summary>
internal static class KernelSharedMemory
{
	internal const int GlobalReduceGroupSize = 256;

	/// <summary>Must divide <see cref="GlobalReduceGroupSize"/> evenly.</summary>
	internal const int VarianceWarpSize = 32;

	internal const int VarianceWarpLeaderSlots = GlobalReduceGroupSize / VarianceWarpSize;

	internal static int VarianceReduceSharedBytes() =>
		VarianceReduceBytes(GlobalReduceGroupSize);

	internal static int VarianceReduceWarpTreeBytes() =>
		VarianceWarpLeaderSlots * (sizeof(int) + sizeof(float) + sizeof(float));

	internal static int VarianceReduceBytes(int groupSize) =>
		groupSize * (sizeof(int) + sizeof(float) + sizeof(float));

	internal enum VarianceReducePath
	{
		SharedScratch,
		WarpTree,
	}

	internal static VarianceReducePath SelectVarianceReducePath(Accelerator accelerator) =>
		SelectVarianceReducePath(accelerator.MaxSharedMemoryPerGroup);

	internal static VarianceReducePath SelectVarianceReducePath(long maxSharedMemoryPerGroup)
	{
		if (VarianceReduceSharedBytes() <= maxSharedMemoryPerGroup)
			return VarianceReducePath.SharedScratch;

		if (VarianceReduceWarpTreeBytes() <= maxSharedMemoryPerGroup)
			return VarianceReducePath.WarpTree;

		throw new NotSupportedException(
			$"Variance reduce requires at least {VarianceReduceWarpTreeBytes()} bytes of shared memory per group; device provides {maxSharedMemoryPerGroup}.");
	}
}
