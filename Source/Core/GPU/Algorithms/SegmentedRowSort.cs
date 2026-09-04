using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.GpuAlgorithms;

/// <summary>
/// Segmented row-wise radix sort: one GPU thread group per matrix row, sorting that row's
/// contiguous segment in a single kernel launch. Caller must pin all backing vectors in
/// <see cref="GpuScope"/> before invoking; this type only dispatches kernels.
/// </summary>
internal static class SegmentedRowSort
{
	internal static void SortIntRows(
		GPU gpu,
		ArrayView1D<int, Stride1D.Dense> view,
		ArrayView<int> tempKeys,
		int rowCount,
		int cols,
		SortOrder order)
	{
		if (cols <= 1)
			return;

		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		if (order == SortOrder.Descending)
		{
			gpu.segmentedSortIntDescIP(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
			return;
		}

		gpu.segmentedSortIntAscIP(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
	}

	internal static void SortFloatRows(
		GPU gpu,
		ArrayView1D<float, Stride1D.Dense> view,
		ArrayView<int> tempKeys,
		int rowCount,
		int cols,
		SortOrder order)
	{
		if (cols <= 1)
			return;

		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		if (order == SortOrder.Descending)
		{
			gpu.segmentedSortFloatDescIP(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
			return;
		}

		gpu.segmentedSortFloatAscIP(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
	}

	internal static void ArgsortIntPairsRows(
		GPU gpu,
		ArrayView1D<int, Stride1D.Dense> keys,
		ArrayView1D<int, Stride1D.Dense> indices,
		ArrayView<int> tempKeys,
		ArrayView<int> tempValues,
		int rowCount,
		int cols,
		SortOrder order)
	{
		if (cols <= 1)
			return;

		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		if (order == SortOrder.Descending)
		{
			gpu.segmentedSortIntPairsDescIP(
				gpu.DefaultStream, config, keys, indices, tempKeys, tempValues, cols, rowCount);
			return;
		}

		gpu.segmentedSortIntPairsAscIP(
			gpu.DefaultStream, config, keys, indices, tempKeys, tempValues, cols, rowCount);
	}

	internal static void ArgsortFloatPairsRows(
		GPU gpu,
		ArrayView1D<float, Stride1D.Dense> floatInput,
		ArrayView1D<int, Stride1D.Dense> keys,
		ArrayView1D<int, Stride1D.Dense> indices,
		ArrayView<int> tempKeys,
		ArrayView<int> tempValues,
		int rowCount,
		int cols,
		SortOrder order)
	{
		if (cols <= 1)
			return;

		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		if (order == SortOrder.Descending)
		{
			gpu.segmentedSortFloatPairsDescIP(
				gpu.DefaultStream, config, floatInput, keys, indices, tempKeys, tempValues, cols, rowCount);
			return;
		}

		gpu.segmentedSortFloatPairsAscIP(
			gpu.DefaultStream, config, floatInput, keys, indices, tempKeys, tempValues, cols, rowCount);
	}
}
