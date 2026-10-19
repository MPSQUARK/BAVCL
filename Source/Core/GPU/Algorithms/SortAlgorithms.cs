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
}
