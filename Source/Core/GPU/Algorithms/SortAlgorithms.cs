using System;
using System.Numerics;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU.Algorithms.Sequencers;
using ILGPU.Runtime;
using BAVCL.Core.Memory;

namespace BAVCL.GpuAlgorithms;

internal static class SortAlgorithms
{
	/// <summary>16 bytes expressed in <see cref="int"/> elements.</summary>
	const int RadixTempAlignmentElements = 4;

	static readonly Int32Sequencer Int32Sequence = new();

	internal static void Sort1DAscXIP(VectorInt vector)
	{
		GPU gpu = vector.Gpu;
		int tempElements = RadixTempElements<AscendingInt32>(gpu, vector.Length);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> temp = BufferPools.For(gpu).Int.Rent(tempElements);
			using (GpuScope.Begin(temp.PinTarget))
			{
				MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.AlgorithmProviders.AscendingIntSort(gpu.DefaultStream, buffer.View, temp.View);
			}

			gpu.Synchronize();
		}
	}

	internal static void Sort1DDescXIP(VectorInt vector)
	{
		GPU gpu = vector.Gpu;
		int tempElements = RadixTempElements<DescendingInt32>(gpu, vector.Length);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> temp = BufferPools.For(gpu).Int.Rent(tempElements);
			using (GpuScope.Begin(temp.PinTarget))
			{
				MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.AlgorithmProviders.DescendingIntSort(gpu.DefaultStream, buffer.View, temp.View);
			}

			gpu.Synchronize();
		}
	}

	internal static void Sort1DAscXIP(Vector vector)
	{
		GPU gpu = vector.Gpu;
		int length = vector.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixTempElements<AscendingInt32>(gpu, length);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
			using (GpuScope.Begin(scratch.PinTarget))
			{
				ArrayView1D<int, Stride1D.Dense>
					scratchView = scratch.View,
					sortable = scratchView.SubView(0, length),
					radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

				MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.floatToSortableInt(gpu.DefaultStream, length, buffer.View, sortable);
				gpu.AlgorithmProviders.AscendingIntSort(gpu.DefaultStream, sortable, radixTemp);
				gpu.sortableIntToFloat(gpu.DefaultStream, length, sortable, buffer.View);
			}

			gpu.Synchronize();
		}
	}

	internal static void Sort1DDescXIP(Vector vector)
	{
		GPU gpu = vector.Gpu;
		int length = vector.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixTempElements<DescendingInt32>(gpu, length);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
			using (GpuScope.Begin(scratch.PinTarget))
			{
				ArrayView1D<int, Stride1D.Dense>
					scratchView = scratch.View,
					sortable = scratchView.SubView(0, length),
					radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);
				MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.floatToSortableInt(gpu.DefaultStream, length, buffer.View, sortable);
				gpu.AlgorithmProviders.DescendingIntSort(gpu.DefaultStream, sortable, radixTemp);
				gpu.sortableIntToFloat(gpu.DefaultStream, length, sortable, buffer.View);
			}

			gpu.Synchronize();
		}
	}

	internal static void Sort2DAscXIP(VectorInt vector)
	{
		int cols = vector.Columns;
		GPU gpu = vector.Gpu;
		int rowCount = vector.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> tempEntity = BufferPools.For(gpu).Int.Rent(vector.Length);
			using (GpuScope.Begin(tempEntity.PinTarget))
			{
				MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.segmentedSortIntAscIP(gpu.DefaultStream, config, buffer.View, tempEntity.View, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	internal static void Sort2DDescXIP(VectorInt vector)
	{
		int cols = vector.Columns;
		GPU gpu = vector.Gpu;
		int rowCount = vector.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> tempEntity = BufferPools.For(gpu).Int.Rent(vector.Length);
			using (GpuScope.Begin(tempEntity.PinTarget))
			{
				MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.segmentedSortIntDescIP(gpu.DefaultStream, config, buffer.View, tempEntity.View, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	internal static void Sort2DAscXIP(Vector vector)
	{
		int cols = vector.Columns;
		GPU gpu = vector.Gpu;
		int rowCount = vector.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> tempEntity = BufferPools.For(gpu).Int.Rent(vector.Length);
			using (GpuScope.Begin(tempEntity.PinTarget))
			{
				MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.segmentedSortFloatAscIP(gpu.DefaultStream, config, buffer.View, tempEntity.View, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	internal static void Sort2DDescXIP(Vector vector)
	{
		int cols = vector.Columns;
		GPU gpu = vector.Gpu;
		int rowCount = vector.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> tempEntity = BufferPools.For(gpu).Int.Rent(vector.Length);
			using (GpuScope.Begin(tempEntity.PinTarget))
			{
				MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
				gpu.segmentedSortFloatDescIP(gpu.DefaultStream, config, buffer.View, tempEntity.View, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort1DAscX(VectorInt input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<AscendingInt32>(gpu, length);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
				using (GpuScope.Begin(scratch.PinTarget))
				{
					ArrayView1D<int, Stride1D.Dense>
						scratchView = scratch.View,
						keys = scratchView.SubView(0, length),
						radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

					MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
					gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
					gpu.AlgorithmProviders.AscendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort1DAscXIP(VectorInt input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<AscendingInt32>(gpu, length);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
			using (GpuScope.Begin(scratch.PinTarget))
			{
				ArrayView1D<int, Stride1D.Dense>
					scratchView = scratch.View,
					keys = scratchView.SubView(0, length),
					radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

				MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
				gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
				gpu.AlgorithmProviders.AscendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort1DDescX(VectorInt input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<DescendingInt32>(gpu, length);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
				using (GpuScope.Begin(scratch.PinTarget))
				{
					ArrayView1D<int, Stride1D.Dense>
						scratchView = scratch.View,
						keys = scratchView.SubView(0, length),
						radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

					MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
					gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
					gpu.AlgorithmProviders.DescendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort1DDescXIP(VectorInt input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<DescendingInt32>(gpu, length);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
			using (GpuScope.Begin(scratch.PinTarget))
			{
				ArrayView1D<int, Stride1D.Dense>
					scratchView = scratch.View,
					keys = scratchView.SubView(0, length),
					radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

				MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
				gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
				gpu.AlgorithmProviders.DescendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort1DAscX(Vector input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<AscendingInt32>(gpu, length);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
				using (GpuScope.Begin(scratch.PinTarget))
				{
					ArrayView1D<int, Stride1D.Dense>
						scratchView = scratch.View,
						keys = scratchView.SubView(0, length),
						radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

					MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					gpu.floatToSortableInt(gpu.DefaultStream, length, inputBuffer.View, keys);
					gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
					gpu.AlgorithmProviders.AscendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort1DAscXIP(Vector input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<AscendingInt32>(gpu, length);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
			using (GpuScope.Begin(scratch.PinTarget))
			{
				ArrayView1D<int, Stride1D.Dense>
					scratchView = scratch.View,
					keys = scratchView.SubView(0, length),
					radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

				MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				gpu.floatToSortableInt(gpu.DefaultStream, length, inputBuffer.View, keys);
				gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
				gpu.AlgorithmProviders.AscendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort1DDescX(Vector input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<DescendingInt32>(gpu, length);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
				using (GpuScope.Begin(scratch.PinTarget))
				{
					ArrayView1D<int, Stride1D.Dense>
						scratchView = scratch.View,
						keys = scratchView.SubView(0, length),
						radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

					MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					gpu.floatToSortableInt(gpu.DefaultStream, length, inputBuffer.View, keys);
					gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
					gpu.AlgorithmProviders.DescendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort1DDescXIP(Vector input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int radixTempOffset = RadixTempOffset(length);
		int radixTempElements = RadixPairsTempElements<DescendingInt32>(gpu, length);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(radixTempOffset + radixTempElements);
			using (GpuScope.Begin(scratch.PinTarget))
			{
				ArrayView1D<int, Stride1D.Dense>
					scratchView = scratch.View,
					keys = scratchView.SubView(0, length),
					radixTemp = scratchView.SubView(radixTempOffset, radixTempElements);

				MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				gpu.floatToSortableInt(gpu.DefaultStream, length, inputBuffer.View, keys);
				gpu.accelerator.Sequence(gpu.DefaultStream, indexBuffer.View, Int32Sequence);
				gpu.AlgorithmProviders.DescendingIntPairsSort(gpu.DefaultStream, keys, indexBuffer.View, radixTemp.AsContiguous());
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort2DAscX(VectorInt input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
				using (GpuScope.Begin(scratch.PinTarget))
				{
					SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
					MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
					gpu.segmentedSortIntPairsAscIP(gpu.DefaultStream, config, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort2DAscXIP(VectorInt input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
			using (GpuScope.Begin(scratch.PinTarget))
			{
				SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
				MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
				gpu.segmentedSortIntPairsAscIP(gpu.DefaultStream, config, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort2DDescX(VectorInt input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
				using (GpuScope.Begin(scratch.PinTarget))
				{
					SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
					MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
					gpu.segmentedSortIntPairsDescIP(gpu.DefaultStream, config, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort2DDescXIP(VectorInt input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
			using (GpuScope.Begin(scratch.PinTarget))
			{
				SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
				MemoryBuffer1D<int, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				inputBuffer.View.CopyTo(gpu.DefaultStream, keys);
				gpu.segmentedSortIntPairsDescIP(gpu.DefaultStream, config, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort2DAscX(Vector input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
				using (GpuScope.Begin(scratch.PinTarget))
				{
					SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
					MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					gpu.segmentedSortFloatPairsAscIP(gpu.DefaultStream, config, inputBuffer.View, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort2DAscXIP(Vector input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
			using (GpuScope.Begin(scratch.PinTarget))
			{
				SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
				MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				gpu.segmentedSortFloatPairsAscIP(gpu.DefaultStream, config, inputBuffer.View, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	internal static VectorInt Argsort2DDescX(Vector input)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);

		// Pin input before allocating indices: the allocation's own LRU pass could otherwise evict it.
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
				using (GpuScope.Begin(scratch.PinTarget))
				{
					SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
					MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
					MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

					gpu.segmentedSortFloatPairsDescIP(gpu.DefaultStream, config, inputBuffer.View, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
				}

				gpu.Synchronize();
			}

			return indices;
		}
	}

	internal static void Argsort2DDescXIP(Vector input, VectorInt indices)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int cols = input.Columns;
		int rowCount = input.RowCount();
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<int> scratch = BufferPools.For(gpu).Int.Rent(Argsort2DScratchElements(length));
			using (GpuScope.Begin(scratch.PinTarget))
			{
				SplitArgsort2DScratch(scratch.View, length, out ArrayView1D<int, Stride1D.Dense> keys, out ArrayView1D<int, Stride1D.Dense> tempKeys, out ArrayView1D<int, Stride1D.Dense> tempValues);
				MemoryBuffer1D<float, Stride1D.Dense> inputBuffer = input.GetBuffer();
				MemoryBuffer1D<int, Stride1D.Dense> indexBuffer = indices.GetBuffer();

				gpu.segmentedSortFloatPairsDescIP(gpu.DefaultStream, config, inputBuffer.View, keys, indexBuffer.View, tempKeys, tempValues, cols, rowCount);
			}

			gpu.Synchronize();
		}
	}

	static int Argsort2DScratchElements(int length) => length + length + length;

	static void SplitArgsort2DScratch(
		ArrayView1D<int, Stride1D.Dense> scratch,
		int length,
		out ArrayView1D<int, Stride1D.Dense> keys,
		out ArrayView1D<int, Stride1D.Dense> tempKeys,
		out ArrayView1D<int, Stride1D.Dense> tempValues)
	{
		keys = scratch.SubView(0, length);
		ArrayView1D<int, Stride1D.Dense> tail = scratch.SubView(length, length << 1);
		tempKeys = tail.SubView(0, length);
		tempValues = tail.SubView(length, length);
	}

	static int RadixTempElements<TSortOp>(GPU gpu, int length) where TSortOp : struct, IRadixSortOperation<int> =>
		(int)gpu.accelerator.ComputeRadixSortTempStorageSize<int, TSortOp>(length);

	static int RadixPairsTempElements<TSortOp>(GPU gpu, int length) where TSortOp : struct, IRadixSortOperation<int> =>
		(int)gpu.accelerator.ComputeRadixSortPairsTempStorageSize<int, int, TSortOp>(length);

	/// <summary>
	/// ILGPU reinterprets radix temp storage as wider structs, so carving it out of a shared scratch
	/// at an unaligned element offset corrupts the sort for lengths that are not a multiple of
	/// <see cref="RadixTempAlignmentElements"/>. Applies to both the keys-only and key/value radix sorts.
	/// </summary>
	static int RadixTempOffset(int length) =>
		(length + RadixTempAlignmentElements - 1) & ~(RadixTempAlignmentElements - 1);

	static int LongPairsTempElements<TSortOp>(GPU gpu, int length) where TSortOp : struct, IRadixSortOperation<long> =>
		(int)gpu.accelerator.ComputeRadixSortPairsTempStorageSize<long, int, TSortOp>(length);

	internal static void Sort1DAscXIP(VectorComplex64 vector, ComplexSortMode mode)
	{
		GPU gpu = vector.Gpu;
		int length = vector.Length;
		int tempElements = LongPairsTempElements<AscendingInt64>(gpu, length);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
			using BufferEntity<int> indices = BufferPools.For(gpu).Int.Rent(length);
			using BufferEntity<int> radixTemp = BufferPools.For(gpu).Int.Rent(tempElements);
			using BufferEntity<Complex> gathered = BufferPools.For(gpu).Complex.Rent(length);
			using (GpuScope.Begin(keys.PinTarget, indices.PinTarget, radixTemp.PinTarget, gathered.PinTarget))
			{
				ArrayView<Complex> values = vector.GetBuffer().View;
				switch (mode)
				{
					case ComplexSortMode.Magnitude:
						gpu.complexMagnitudeKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Real:
						gpu.complexRealKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Imaginary:
					case ComplexSortMode.Complex:
						gpu.complexImaginaryKey(gpu.DefaultStream, length, values, keys.View);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode));
				}
				gpu.accelerator.Sequence(gpu.DefaultStream, indices.View, Int32Sequence);
				gpu.AlgorithmProviders.AscendingLongPairsSort(gpu.DefaultStream, keys.View, indices.View, radixTemp.View);
				if (mode == ComplexSortMode.Complex)
				{
					gpu.complexIndexedRealKey(gpu.DefaultStream, length, values, indices.View, keys.View, length);
					gpu.AlgorithmProviders.AscendingLongPairsSort(gpu.DefaultStream, keys.View, indices.View, radixTemp.View);
				}
				gpu.complexGatherByIndex(gpu.DefaultStream, length, gathered.View, values, indices.View);
				gathered.View.CopyTo(gpu.DefaultStream, values);
				gpu.Synchronize();
			}
		}
	}

	internal static void Sort1DDescXIP(VectorComplex64 vector, ComplexSortMode mode)
	{
		GPU gpu = vector.Gpu;
		int length = vector.Length;
		int tempElements = LongPairsTempElements<DescendingInt64>(gpu, length);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
			using BufferEntity<int> indices = BufferPools.For(gpu).Int.Rent(length);
			using BufferEntity<int> radixTemp = BufferPools.For(gpu).Int.Rent(tempElements);
			using BufferEntity<Complex> gathered = BufferPools.For(gpu).Complex.Rent(length);
			using (GpuScope.Begin(keys.PinTarget, indices.PinTarget, radixTemp.PinTarget, gathered.PinTarget))
			{
				ArrayView<Complex> values = vector.GetBuffer().View;
				switch (mode)
				{
					case ComplexSortMode.Magnitude:
						gpu.complexMagnitudeKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Real:
						gpu.complexRealKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Imaginary:
					case ComplexSortMode.Complex:
						gpu.complexImaginaryKey(gpu.DefaultStream, length, values, keys.View);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode));
				}
				gpu.accelerator.Sequence(gpu.DefaultStream, indices.View, Int32Sequence);
				gpu.AlgorithmProviders.DescendingLongPairsSort(gpu.DefaultStream, keys.View, indices.View, radixTemp.View);
				if (mode == ComplexSortMode.Complex)
				{
					gpu.complexIndexedRealKey(gpu.DefaultStream, length, values, indices.View, keys.View, length);
					gpu.AlgorithmProviders.DescendingLongPairsSort(gpu.DefaultStream, keys.View, indices.View, radixTemp.View);
				}
				gpu.complexGatherByIndex(gpu.DefaultStream, length, gathered.View, values, indices.View);
				gathered.View.CopyTo(gpu.DefaultStream, values);
				gpu.Synchronize();
			}
		}
	}

	internal static void Sort2DAscXIP(VectorComplex64 vector, ComplexSortMode mode) =>
		Sort2D(vector, mode, static (gpu, config, keys, values, tempKeys, tempValues, columns, rowCount) =>
			gpu.segmentedSortComplexPairsAscIP(gpu.DefaultStream, config, keys, values, tempKeys, tempValues, columns, rowCount));

	internal static void Sort2DDescXIP(VectorComplex64 vector, ComplexSortMode mode) =>
		Sort2D(vector, mode, static (gpu, config, keys, values, tempKeys, tempValues, columns, rowCount) =>
			gpu.segmentedSortComplexPairsDescIP(gpu.DefaultStream, config, keys, values, tempKeys, tempValues, columns, rowCount));

	internal static VectorInt Argsort1DAscX(VectorComplex64 input, ComplexSortMode mode)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int tempElements = LongPairsTempElements<AscendingInt64>(gpu, length);
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
				using BufferEntity<int> radixTemp = BufferPools.For(gpu).Int.Rent(tempElements);
				using (GpuScope.Begin(keys.PinTarget, radixTemp.PinTarget))
				{
					ArrayView<Complex> values = input.GetBuffer().View;
					ArrayView<int> order = indices.GetBuffer().View;
					switch (mode)
				{
					case ComplexSortMode.Magnitude:
						gpu.complexMagnitudeKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Real:
						gpu.complexRealKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Imaginary:
					case ComplexSortMode.Complex:
						gpu.complexImaginaryKey(gpu.DefaultStream, length, values, keys.View);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode));
				}
					gpu.accelerator.Sequence(gpu.DefaultStream, order, Int32Sequence);
					gpu.AlgorithmProviders.AscendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
					if (mode == ComplexSortMode.Complex)
					{
						gpu.complexIndexedRealKey(gpu.DefaultStream, length, values, order, keys.View, length);
						gpu.AlgorithmProviders.AscendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
					}
					gpu.Synchronize();
				}
			}
			return indices;
		}
	}

	internal static VectorInt Argsort1DDescX(VectorComplex64 input, ComplexSortMode mode)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int tempElements = LongPairsTempElements<DescendingInt64>(gpu, length);
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, length, input.Columns);
			using (GpuScope.Begin(indices))
			{
				using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
				using BufferEntity<int> radixTemp = BufferPools.For(gpu).Int.Rent(tempElements);
				using (GpuScope.Begin(keys.PinTarget, radixTemp.PinTarget))
				{
					ArrayView<Complex> values = input.GetBuffer().View;
					ArrayView<int> order = indices.GetBuffer().View;
					switch (mode)
				{
					case ComplexSortMode.Magnitude:
						gpu.complexMagnitudeKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Real:
						gpu.complexRealKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Imaginary:
					case ComplexSortMode.Complex:
						gpu.complexImaginaryKey(gpu.DefaultStream, length, values, keys.View);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode));
				}
					gpu.accelerator.Sequence(gpu.DefaultStream, order, Int32Sequence);
					gpu.AlgorithmProviders.DescendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
					if (mode == ComplexSortMode.Complex)
					{
						gpu.complexIndexedRealKey(gpu.DefaultStream, length, values, order, keys.View, length);
						gpu.AlgorithmProviders.DescendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
					}
					gpu.Synchronize();
				}
			}
			return indices;
		}
	}

	internal static void Argsort1DAscXIP(VectorComplex64 input, VectorInt indices, ComplexSortMode mode)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int tempElements = LongPairsTempElements<AscendingInt64>(gpu, length);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
			using BufferEntity<int> radixTemp = BufferPools.For(gpu).Int.Rent(tempElements);
			using (GpuScope.Begin(keys.PinTarget, radixTemp.PinTarget))
			{
				ArrayView<Complex> values = input.GetBuffer().View;
				ArrayView<int> order = indices.GetBuffer().View;
				switch (mode)
				{
					case ComplexSortMode.Magnitude:
						gpu.complexMagnitudeKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Real:
						gpu.complexRealKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Imaginary:
					case ComplexSortMode.Complex:
						gpu.complexImaginaryKey(gpu.DefaultStream, length, values, keys.View);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode));
				}
				gpu.accelerator.Sequence(gpu.DefaultStream, order, Int32Sequence);
				gpu.AlgorithmProviders.AscendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
				if (mode == ComplexSortMode.Complex)
				{
					gpu.complexIndexedRealKey(gpu.DefaultStream, length, values, order, keys.View, length);
					gpu.AlgorithmProviders.AscendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
				}
				gpu.Synchronize();
			}
		}
	}

	internal static void Argsort1DDescXIP(VectorComplex64 input, VectorInt indices, ComplexSortMode mode)
	{
		GPU gpu = input.Gpu;
		int length = input.Length;
		int tempElements = LongPairsTempElements<DescendingInt64>(gpu, length);
		using (GpuScope.Begin(indices, input))
		{
			using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
			using BufferEntity<int> radixTemp = BufferPools.For(gpu).Int.Rent(tempElements);
			using (GpuScope.Begin(keys.PinTarget, radixTemp.PinTarget))
			{
				ArrayView<Complex> values = input.GetBuffer().View;
				ArrayView<int> order = indices.GetBuffer().View;
				switch (mode)
				{
					case ComplexSortMode.Magnitude:
						gpu.complexMagnitudeKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Real:
						gpu.complexRealKey(gpu.DefaultStream, length, values, keys.View);
						break;
					case ComplexSortMode.Imaginary:
					case ComplexSortMode.Complex:
						gpu.complexImaginaryKey(gpu.DefaultStream, length, values, keys.View);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode));
				}
				gpu.accelerator.Sequence(gpu.DefaultStream, order, Int32Sequence);
				gpu.AlgorithmProviders.DescendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
				if (mode == ComplexSortMode.Complex)
				{
					gpu.complexIndexedRealKey(gpu.DefaultStream, length, values, order, keys.View, length);
					gpu.AlgorithmProviders.DescendingLongPairsSort(gpu.DefaultStream, keys.View, order, radixTemp.View);
				}
				gpu.Synchronize();
			}
		}
	}

	internal static VectorInt Argsort2DAscX(VectorComplex64 input, ComplexSortMode mode) =>
		Argsort2D(input, mode, static (gpu, config, keys, indices, tempKeys, tempIndices, columns, rowCount, seed) =>
			gpu.segmentedArgsortComplexPairsAscIP(
				gpu.DefaultStream, config, keys, indices, tempKeys, tempIndices, columns, rowCount, seed));

	internal static VectorInt Argsort2DDescX(VectorComplex64 input, ComplexSortMode mode) =>
		Argsort2D(input, mode, static (gpu, config, keys, indices, tempKeys, tempIndices, columns, rowCount, seed) =>
			gpu.segmentedArgsortComplexPairsDescIP(
				gpu.DefaultStream, config, keys, indices, tempKeys, tempIndices, columns, rowCount, seed));

	internal static void Argsort2DAscXIP(VectorComplex64 input, VectorInt indices, ComplexSortMode mode) =>
		Argsort2DInto(input, indices, mode, static (gpu, config, keys, order, tempKeys, tempIndices, columns, rowCount, seed) =>
			gpu.segmentedArgsortComplexPairsAscIP(
				gpu.DefaultStream, config, keys, order, tempKeys, tempIndices, columns, rowCount, seed));

	internal static void Argsort2DDescXIP(VectorComplex64 input, VectorInt indices, ComplexSortMode mode) =>
		Argsort2DInto(input, indices, mode, static (gpu, config, keys, order, tempKeys, tempIndices, columns, rowCount, seed) =>
			gpu.segmentedArgsortComplexPairsDescIP(
				gpu.DefaultStream, config, keys, order, tempKeys, tempIndices, columns, rowCount, seed));

	static void Sort2D(
		VectorComplex64 vector,
		ComplexSortMode mode,
		Action<GPU, (int, int), ArrayView<long>, ArrayView<Complex>, ArrayView<long>, ArrayView<Complex>, int, int> sortPairs)
	{
		GPU gpu = vector.Gpu;
		int columns = vector.Columns;
		int rowCount = vector.RowCount();
		int length = vector.Length;
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
			using BufferEntity<long> tempKeys = BufferPools.For(gpu).Long.Rent(length);
			using BufferEntity<Complex> tempValues = BufferPools.For(gpu).Complex.Rent(length);
			using (GpuScope.Begin(keys.PinTarget, tempKeys.PinTarget, tempValues.PinTarget))
			{
				ArrayView<Complex> values = vector.GetBuffer().View;
				LaunchComponentKey(gpu, mode, length, values, keys.View);
				sortPairs(gpu, config, keys.View, values, tempKeys.View, tempValues.View, columns, rowCount);
				if (mode == ComplexSortMode.Complex)
				{
					gpu.complexRealKey(gpu.DefaultStream, length, values, keys.View);
					sortPairs(gpu, config, keys.View, values, tempKeys.View, tempValues.View, columns, rowCount);
				}

				gpu.Synchronize();
			}
		}
	}

	static VectorInt Argsort2D(
		VectorComplex64 input,
		ComplexSortMode mode,
		Action<GPU, (int, int), ArrayView<long>, ArrayView<int>, ArrayView<long>, ArrayView<int>, int, int, int> sortPairs)
	{
		GPU gpu = input.Gpu;
		using (GpuScope.BeginReadOnly(input))
		{
			VectorInt indices = new(gpu, input.Length, input.Columns);
			using (GpuScope.Begin(indices))
				Argsort2DCore(input, indices, mode, sortPairs);
			return indices;
		}
	}

	static void Argsort2DInto(
		VectorComplex64 input,
		VectorInt indices,
		ComplexSortMode mode,
		Action<GPU, (int, int), ArrayView<long>, ArrayView<int>, ArrayView<long>, ArrayView<int>, int, int, int> sortPairs)
	{
		using (GpuScope.Begin(indices, input))
			Argsort2DCore(input, indices, mode, sortPairs);
	}

	static void Argsort2DCore(
		VectorComplex64 input,
		VectorInt indices,
		ComplexSortMode mode,
		Action<GPU, (int, int), ArrayView<long>, ArrayView<int>, ArrayView<long>, ArrayView<int>, int, int, int> sortPairs)
	{
		GPU gpu = input.Gpu;
		int columns = input.Columns;
		int rowCount = input.RowCount();
		int length = input.Length;
		var config = (rowCount, GPU.SegmentedRadixGroupSize);
		using BufferEntity<long> keys = BufferPools.For(gpu).Long.Rent(length);
		using BufferEntity<long> tempKeys = BufferPools.For(gpu).Long.Rent(length);
		using BufferEntity<int> tempIndices = BufferPools.For(gpu).Int.Rent(length);
		using (GpuScope.Begin(keys.PinTarget, tempKeys.PinTarget, tempIndices.PinTarget))
		{
			ArrayView<Complex> values = input.GetBuffer().View;
			ArrayView<int> order = indices.GetBuffer().View;
			LaunchComponentKey(gpu, mode, length, values, keys.View);
			sortPairs(gpu, config, keys.View, order, tempKeys.View, tempIndices.View, columns, rowCount, 1);
			if (mode == ComplexSortMode.Complex)
			{
				gpu.complexIndexedRealKey(gpu.DefaultStream, length, values, order, keys.View, columns);
				sortPairs(gpu, config, keys.View, order, tempKeys.View, tempIndices.View, columns, rowCount, 0);
			}

			gpu.Synchronize();
		}
	}

	static void LaunchComponentKey(GPU gpu, ComplexSortMode mode, int length, ArrayView<Complex> values, ArrayView<long> keys)
	{
		switch (mode)
		{
			case ComplexSortMode.Magnitude:
				gpu.complexMagnitudeKey(gpu.DefaultStream, length, values, keys);
				return;
			case ComplexSortMode.Real:
				gpu.complexRealKey(gpu.DefaultStream, length, values, keys);
				return;
			case ComplexSortMode.Imaginary:
			case ComplexSortMode.Complex:
				gpu.complexImaginaryKey(gpu.DefaultStream, length, values, keys);
				return;
			default:
				throw new ArgumentOutOfRangeException(nameof(mode));
		}
	}
}

