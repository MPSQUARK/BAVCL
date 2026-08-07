using System;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.Sequencers;
using ILGPU.Runtime;
using BAVCL.Core;
using BAVCL.Core.Exceptions;

namespace BAVCL.GpuAlgorithms;

internal static class SortAlgorithms
{
	static readonly Int32Sequencer Int32Sequence = new();

	internal static void SortIntXIP(VectorInt vector, SortOrder order)
	{
		if (vector.Length <= 1)
			return;

		if (vector.Is2D())
		{
			SortIntSegmentedXIP(vector, order);
			return;
		}

		GPU gpu = vector.Gpu;
		using (GpuScope.Begin(vector))
		{
			SortIntSegment(gpu, vector.GetBuffer().View, order);
			gpu.Synchronize();
		}
	}

	internal static void SortFloatXIP(Vector vector, SortOrder order)
	{
		if (vector.Length <= 1)
			return;

		GPU gpu = vector.Gpu;

		if (vector.Is2D())
		{
			SortFloatSegmentedXIP(vector, order);
			return;
		}

		using (GpuScope.Begin(vector))
		{
			SortFloatSegment(gpu, vector.GetBuffer().View, vector.Columns, order);
			gpu.Synchronize();
		}
	}

	internal static VectorInt ArgsortIntX(VectorInt input, SortOrder order)
	{
		if (input.Length == 0)
			return new VectorInt(input.Gpu, 0, input.Columns);

		GPU gpu = input.Gpu;
		// Pin input before allocating indices: `new VectorInt` allocates eagerly in its constructor,
		// and that allocation's own LRU.GC pass could otherwise evict input while it still sits unpinned.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, input.Length, input.Columns);
			using (GpuScope.Begin(indices))
				RunArgsortInt(gpu, input, indices, order);

			return indices;
		}
	}

	internal static void ArgsortIntXIP(VectorInt input, VectorInt indices, SortOrder order)
	{
		ValidateArgsortShape(input, indices);

		if (input.Length == 0)
			return;

		GPU gpu = input.Gpu;
		using (GpuScope.BeginReadOnly(input))
		using (GpuScope.Begin(indices))
			RunArgsortInt(gpu, input, indices, order);
	}

	internal static VectorInt ArgsortFloatX(Vector input, SortOrder order)
	{
		if (input.Length == 0)
			return new VectorInt(input.Gpu, 0, input.Columns);

		GPU gpu = input.Gpu;

		// Pin input before allocating indices: `new VectorInt` allocates eagerly in its constructor,
		// and that allocation's own LRU.GC pass could otherwise evict input while it still sits unpinned.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, input.Length, input.Columns);
			using (GpuScope.Begin(indices))
				RunArgsortFloat(gpu, input, indices, order);

			return indices;
		}
	}

	internal static void ArgsortFloatXIP(Vector input, VectorInt indices, SortOrder order)
	{
		ValidateArgsortShape(input, indices);

		if (input.Length == 0)
			return;

		GPU gpu = input.Gpu;

		using (GpuScope.BeginReadOnly(input))
		using (GpuScope.Begin(indices))
			RunArgsortFloat(gpu, input, indices, order);
	}

	static void RunArgsortInt(GPU gpu, VectorInt input, VectorInt indices, SortOrder order)
	{
		if (input.Is2D())
		{
			RunArgsortIntSegmented(gpu, input, indices, order);
			return;
		}

		using SortScratchLease keysLease = gpu.RentIntScratch(input.Length, input.Columns);
		using (GpuScope.Begin(keysLease.Vector))
		{
			ArgsortIntSegment(gpu, input.GetBuffer().View, indices.GetBuffer().View, keysLease.View, order);
			gpu.Synchronize();
		}
	}

	static void RunArgsortFloat(GPU gpu, Vector input, VectorInt indices, SortOrder order)
	{
		if (input.Is2D())
		{
			RunArgsortFloatSegmented(gpu, input, indices, order);
			return;
		}

		using SortScratchLease keysLease = gpu.RentIntScratch(input.Length, input.Columns);
		using (GpuScope.Begin(keysLease.Vector))
		{
			ArgsortFloatSegment(gpu, input.GetBuffer().View, indices.GetBuffer().View, keysLease.View, order);
			gpu.Synchronize();
		}
	}

	static void SortIntSegmentedXIP(VectorInt vector, SortOrder order)
	{
		GPU gpu = vector.Gpu;
		int rowCount = vector.RowCount(), cols = vector.Columns;
		// Pin vector before renting temp: renting may allocate, and that allocation's own LRU.GC
		// pass could otherwise evict vector while it still sits unpinned.
		using (GpuScope.Begin(vector))
		{
			using SortScratchLease tempLease = gpu.RentSegmentedTemp(vector.Length);
			using (GpuScope.Begin(tempLease.Vector))
			{
				var (tempKeys, _) = GPU.SegmentedSortTempViews(tempLease.Vector, vector.Length, SortTempLayout.KeysOnly);
				SegmentedRowSort.SortIntRows(
					gpu,
					vector.GetBuffer().View,
					tempKeys,
					rowCount,
					cols,
					order);
				gpu.Synchronize();
			}
		}
	}

	static void SortFloatSegmentedXIP(Vector vector, SortOrder order)
	{
		GPU gpu = vector.Gpu;
		int rowCount = vector.RowCount(), cols = vector.Columns;
		using (GpuScope.Begin(vector))
		{
			using SortScratchLease tempLease = gpu.RentSegmentedTemp(vector.Length);
			using (GpuScope.Begin(tempLease.Vector))
			{
				var (tempKeys, _) = GPU.SegmentedSortTempViews(tempLease.Vector, vector.Length, SortTempLayout.KeysOnly);
				SegmentedRowSort.SortFloatRows(
					gpu,
					vector.GetBuffer().View,
					tempKeys,
					rowCount,
					cols,
					order);
				gpu.Synchronize();
			}
		}
	}

	static void RunArgsortIntSegmented(GPU gpu, VectorInt input, VectorInt indices, SortOrder order)
	{
		int rowCount = input.RowCount(), cols = input.Columns;
		// Pin each newly-rented scratch object immediately after creation: renting may allocate, and
		// that allocation's own LRU.GC pass could otherwise evict an earlier, still-unpinned object.
		using SortScratchLease keysLease = gpu.RentIntScratch(input.Length, input.Columns);
		using (GpuScope.Begin(keysLease.Vector))
		{
			using SortScratchLease tempLease = gpu.RentSegmentedPairsTemp(input.Length);
			using (GpuScope.Begin(tempLease.Vector))
			{
				var keysView = keysLease.View;
				// Segmented radix sort mutates keys in place; copy so the caller's input is left untouched.
				input.GetBuffer().View.CopyTo(gpu.DefaultStream, keysView);
				var (tempKeys, tempValues) = GPU.SegmentedSortTempViews(tempLease.Vector, input.Length, SortTempLayout.KeysAndValues);
				SegmentedRowSort.ArgsortIntPairsRows(
					gpu, keysView, indices.GetBuffer().View, tempKeys, tempValues, rowCount, cols, order);
				gpu.Synchronize();
			}
		}
	}

	static void RunArgsortFloatSegmented(GPU gpu, Vector input, VectorInt indices, SortOrder order)
	{
		int rowCount = input.RowCount(), cols = input.Columns;
		// Pin each newly-rented scratch object immediately after creation: renting may allocate, and
		// that allocation's own LRU.GC pass could otherwise evict an earlier, still-unpinned object.
		using SortScratchLease keysLease = gpu.RentIntScratch(input.Length, input.Columns);
		using (GpuScope.Begin(keysLease.Vector))
		{
			using SortScratchLease tempLease = gpu.RentSegmentedPairsTemp(input.Length);
			using (GpuScope.Begin(tempLease.Vector))
			{
				var (tempKeys, tempValues) = GPU.SegmentedSortTempViews(tempLease.Vector, input.Length, SortTempLayout.KeysAndValues);
				SegmentedRowSort.ArgsortFloatPairsRows(
					gpu,
					input.GetBuffer().View,
					keysLease.View,
					indices.GetBuffer().View,
					tempKeys,
					tempValues,
					rowCount,
					cols,
					order);
				gpu.Synchronize();
			}
		}
	}

	static void ValidateArgsortShape<T>(VectorBase<T> input, VectorInt indices) where T : unmanaged
	{
		if (!input.Shape().MatchesDimensions(indices.Shape()))
			throw new ShapeMismatchException("ArgsortXIP", input.Shape(), indices.Shape());
	}

	#region Sort — int

	static void SortIntSegment(GPU gpu, ArrayView1D<int, Stride1D.Dense> segment, SortOrder order)
	{
		if (segment.Length <= 1)
			return;

		if (order == SortOrder.Ascending)
		{
			gpu.AlgorithmProviders.GetAscendingIntSort(segment.Length)(gpu.DefaultStream, segment);
			return;
		}

		gpu.AlgorithmProviders.GetDescendingIntSort(segment.Length)(gpu.DefaultStream, segment);
	}

	#endregion

	#region Sort — float

	static void SortFloatSegment(
		GPU gpu,
		ArrayView1D<float, Stride1D.Dense> segment,
		int columns,
		SortOrder order)
	{
		if (segment.Length <= 1)
			return;

		using SortScratchLease sortableLease = gpu.RentIntScratch((int)segment.Length, columns);
		using (GpuScope.Begin(sortableLease.Vector))
			RunFloatSortViaSortable(gpu, segment, sortableLease.View, order);
	}

	static void RunFloatSortViaSortable(
		GPU gpu,
		ArrayView1D<float, Stride1D.Dense> segment,
		ArrayView1D<int, Stride1D.Dense> sortableView,
		SortOrder order)
	{
		Index1D extent = GPU.SortLaunchExtent(segment.Length);
		gpu.floatToSortableIntKern(gpu.DefaultStream, extent, segment, sortableView);
		SortIntSegment(gpu, sortableView, order);
		gpu.sortableIntToFloatKern(gpu.DefaultStream, extent, sortableView, segment);
	}

	#endregion

	#region Argsort — int

	static void ArgsortIntSegment(
		GPU gpu,
		ArrayView1D<int, Stride1D.Dense> inputView,
		ArrayView1D<int, Stride1D.Dense> indexView,
		ArrayView1D<int, Stride1D.Dense> keysView,
		SortOrder order)
	{
		// ILGPU pairs radix sort mutates keys in place; copy so the caller's input is left untouched.
		inputView.CopyTo(gpu.DefaultStream, keysView);
		gpu.accelerator.Sequence(gpu.DefaultStream, indexView, Int32Sequence);
		ArgsortIntPairs(gpu, keysView, indexView, order);
	}

	static void ArgsortIntPairs(
		GPU gpu,
		ArrayView1D<int, Stride1D.Dense> keys,
		ArrayView1D<int, Stride1D.Dense> values,
		SortOrder order)
	{
		if (order == SortOrder.Ascending)
		{
			gpu.AlgorithmProviders.GetAscendingIntPairsSort(keys.Length)(gpu.DefaultStream, keys, values);
			return;
		}

		gpu.AlgorithmProviders.GetDescendingIntPairsSort(keys.Length)(gpu.DefaultStream, keys, values);
	}

	#endregion

	#region Argsort — float

	static void ArgsortFloatSegment(
		GPU gpu,
		ArrayView1D<float, Stride1D.Dense> inputView,
		ArrayView1D<int, Stride1D.Dense> indexView,
		ArrayView1D<int, Stride1D.Dense> keysView,
		SortOrder order) =>
		RunArgsortFloatViaSortable(gpu, inputView, indexView, keysView, order);

	static void RunArgsortFloatViaSortable(
		GPU gpu,
		ArrayView1D<float, Stride1D.Dense> inputView,
		ArrayView1D<int, Stride1D.Dense> indexView,
		ArrayView1D<int, Stride1D.Dense> keysView,
		SortOrder order)
	{
		gpu.floatToSortableIntKern(gpu.DefaultStream, GPU.SortLaunchExtent(inputView.Length), inputView, keysView);
		gpu.accelerator.Sequence(gpu.DefaultStream, indexView, Int32Sequence);
		ArgsortIntPairs(gpu, keysView, indexView, order);
	}

	#endregion
}
