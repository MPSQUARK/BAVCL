using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	internal const int SegmentedRadixBuckets = 256;
	internal const int SegmentedRadixPasses = 4;
	internal const int SegmentedRadixGroupSize = 256;

	public Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, int, int> segmentedSortIntAscKern
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortIntAscKern));

	public Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, int, int> segmentedSortIntDescKern
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortIntDescKern));

	public Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>
		segmentedSortIntPairsAscKern
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortIntPairsAscKern));

	public Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>
		segmentedSortIntPairsDescKern
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortIntPairsDescKern));

	internal void LoadSortSegmentedRadixKernels()
	{
		segmentedSortIntAscKern = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int, int>(SegmentedSortIntRowKern<AscendingInt32>);
		segmentedSortIntDescKern = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int, int>(SegmentedSortIntRowKern<DescendingInt32>);
		segmentedSortIntPairsAscKern = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(
			SegmentedSortIntPairsRowKern<AscendingInt32>);
		segmentedSortIntPairsDescKern = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(
			SegmentedSortIntPairsRowKern<DescendingInt32>);
	}

	/// <summary>
	/// One grid group per matrix row; threads in the group cooperatively radix-sort that row's segment.
	/// Host launches exactly <c>rowCount</c> groups and guards <c>cols &lt;= 1</c>, so no bounds check is needed here.
	/// </summary>
	static void SegmentedSortIntRowKern<TOperation>(
		ArrayView<int> input,
		ArrayView<int> temp,
		int cols,
		int rowCount)
		where TOperation : struct, IRadixSortOperation<int>
	{
		int rowStart = Grid.IdxX * cols;
		TOperation operation = default;
		var histogram = SharedMemory.Allocate<int>(SegmentedRadixBuckets);
		var bucketStarts = SharedMemory.Allocate<int>(SegmentedRadixBuckets);

		for (int pass = 0; pass < SegmentedRadixPasses; pass++)
		{
			int shift = pass << 3;
			ArrayView<int> readView, writeView;
			if ((pass & 1) == 0)
			{
				readView = input;
				writeView = temp;
			}
			else
			{
				readView = temp;
				writeView = input;
			}

			// Histogram: count digit occurrences for this pass's byte.
			for (int bucket = Group.IdxX; bucket < SegmentedRadixBuckets; bucket += Group.DimX)
				histogram[bucket] = 0;
			Group.Barrier();

			for (int i = Group.IdxX; i < cols; i += Group.DimX)
			{
				int digit = operation.ExtractRadixBits(readView[rowStart + i], shift, SegmentedRadixBuckets - 1);
				Atomic.Add(ref histogram[digit], 1);
			}
			Group.Barrier();

			// Prefix: turn per-digit counts into exclusive bucket start offsets.
			if (Group.IdxX == 0)
			{
				int sum = 0;
				for (int bucket = 0; bucket < SegmentedRadixBuckets; bucket++)
				{
					bucketStarts[bucket] = sum;
					sum += histogram[bucket];
				}
			}
			Group.Barrier();

			for (int bucket = Group.IdxX; bucket < SegmentedRadixBuckets; bucket += Group.DimX)
				histogram[bucket] = bucketStarts[bucket];
			Group.Barrier();

			// Scatter: move each element into its bucket-relative slot in the write buffer.
			for (int i = Group.IdxX; i < cols; i += Group.DimX)
			{
				int srcIdx = rowStart + i;
				int value = readView[srcIdx];
				int digit = operation.ExtractRadixBits(value, shift, SegmentedRadixBuckets - 1);
				int dstIdx = rowStart + Atomic.Add(ref histogram[digit], 1);
				writeView[dstIdx] = value;
			}
			Group.Barrier();

			// Buffers swap implicitly next iteration via `pass` parity; SegmentedRadixPasses is even,
			// so the final pass always writes back into `input`.
		}
	}

	/// <summary>
	/// One grid group per matrix row; sorts <paramref name="keys"/> and carries <paramref name="values"/> along.
	/// Seeds <paramref name="values"/> with each row's local index sequence itself, so no separate init
	/// kernel launch is needed before the sort. Host launches exactly <c>rowCount</c> groups and guards
	/// <c>cols &lt;= 1</c>, so no bounds check is needed here.
	/// </summary>
	static void SegmentedSortIntPairsRowKern<TOperation>(
		ArrayView<int> keys,
		ArrayView<int> values,
		ArrayView<int> tempKeys,
		ArrayView<int> tempValues,
		int cols,
		int rowCount)
		where TOperation : struct, IRadixSortOperation<int>
	{
		int rowStart = Grid.IdxX * cols;
		TOperation operation = default;
		var histogram = SharedMemory.Allocate<int>(SegmentedRadixBuckets);
		var bucketStarts = SharedMemory.Allocate<int>(SegmentedRadixBuckets);

		for (int i = Group.IdxX; i < cols; i += Group.DimX)
			values[rowStart + i] = i;
		Group.Barrier();

		for (int pass = 0; pass < SegmentedRadixPasses; pass++)
		{
			int shift = pass << 3;
			ArrayView<int> readKeys, writeKeys, readValues, writeValues;
			if ((pass & 1) == 0)
			{
				readKeys = keys;
				writeKeys = tempKeys;
				readValues = values;
				writeValues = tempValues;
			}
			else
			{
				readKeys = tempKeys;
				writeKeys = keys;
				readValues = tempValues;
				writeValues = values;
			}

			// Histogram: count digit occurrences for this pass's byte.
			for (int bucket = Group.IdxX; bucket < SegmentedRadixBuckets; bucket += Group.DimX)
				histogram[bucket] = 0;
			Group.Barrier();

			for (int i = Group.IdxX; i < cols; i += Group.DimX)
			{
				int digit = operation.ExtractRadixBits(readKeys[rowStart + i], shift, SegmentedRadixBuckets - 1);
				Atomic.Add(ref histogram[digit], 1);
			}
			Group.Barrier();

			// Prefix: turn per-digit counts into exclusive bucket start offsets.
			if (Group.IdxX == 0)
			{
				int sum = 0;
				for (int bucket = 0; bucket < SegmentedRadixBuckets; bucket++)
				{
					bucketStarts[bucket] = sum;
					sum += histogram[bucket];
				}
			}
			Group.Barrier();

			for (int bucket = Group.IdxX; bucket < SegmentedRadixBuckets; bucket += Group.DimX)
				histogram[bucket] = bucketStarts[bucket];
			Group.Barrier();

			// Scatter: move each key/value pair into its bucket-relative slot in the write buffers.
			for (int i = Group.IdxX; i < cols; i += Group.DimX)
			{
				int srcIdx = rowStart + i;
				int key = readKeys[srcIdx];
				int value = readValues[srcIdx];
				int digit = operation.ExtractRadixBits(key, shift, SegmentedRadixBuckets - 1);
				int dstIdx = rowStart + Atomic.Add(ref histogram[digit], 1);
				writeKeys[dstIdx] = key;
				writeValues[dstIdx] = value;
			}
			Group.Barrier();

			// Buffers swap implicitly next iteration via `pass` parity; SegmentedRadixPasses is even,
			// so the final pass always writes back into `keys` / `values`.
		}
	}
}
