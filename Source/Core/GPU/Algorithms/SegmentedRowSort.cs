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
			gpu.segmentedSortIntDescKern(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
			return;
		}

		gpu.segmentedSortIntAscKern(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
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
			gpu.segmentedSortFloatDescKern(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
			return;
		}

		gpu.segmentedSortFloatAscKern(gpu.DefaultStream, config, view, tempKeys, cols, rowCount);
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
			gpu.segmentedSortIntPairsDescKern(
				gpu.DefaultStream, config, keys, indices, tempKeys, tempValues, cols, rowCount);
			return;
		}

		gpu.segmentedSortIntPairsAscKern(
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
			gpu.segmentedSortFloatPairsDescKern(
				gpu.DefaultStream, config, floatInput, keys, indices, tempKeys, tempValues, cols, rowCount);
			return;
		}

		gpu.segmentedSortFloatPairsAscKern(
			gpu.DefaultStream, config, floatInput, keys, indices, tempKeys, tempValues, cols, rowCount);
	}
}
