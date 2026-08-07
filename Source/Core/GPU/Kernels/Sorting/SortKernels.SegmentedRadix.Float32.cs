using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<int>, int, int> segmentedSortFloatAscKern
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortFloatAscKern));

	public Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<int>, int, int> segmentedSortFloatDescKern
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortFloatDescKern));

	public Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>
		segmentedSortFloatPairsAscKern
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortFloatPairsAscKern));

	public Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>
		segmentedSortFloatPairsDescKern
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortFloatPairsDescKern));

	internal void LoadSortSegmentedFloatRadixKernels()
	{
		segmentedSortFloatAscKern = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<int>, int, int>(SegmentedSortFloatRowKern<AscendingInt32>);
		segmentedSortFloatDescKern = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<int>, int, int>(SegmentedSortFloatRowKern<DescendingInt32>);
		segmentedSortFloatPairsAscKern = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(
			SegmentedSortFloatPairsRowKern<AscendingInt32>);
		segmentedSortFloatPairsDescKern = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(
			SegmentedSortFloatPairsRowKern<DescendingInt32>);
	}

	/// <summary>
	/// Branchless float→sortable-int mapping shared by conversion kernels and fused segmented sort.
	/// </summary>
	static int FloatBitsToSortable(int bits) => bits ^ ((bits >> 31) & int.MaxValue);

	static int SortableToFloatBits(int sortableBits) => sortableBits ^ ((sortableBits >> 31) & int.MaxValue);

	static int ReadFloatSortableKey(ArrayView<float> floatView, int index, int pass)
	{
		int bits = (int)Interop.FloatAsInt(floatView[index]);
		// Pass 0 reads native floats; middle passes read sortable-int bit patterns stored via IntAsFloat.
		return pass == 0 ? FloatBitsToSortable(bits) : bits;
	}

	static int ReadIntSortableKey(ArrayView<int> intView, int index) => intView[index];

	static void WriteFloatSortableKey(ArrayView<float> floatView, int index, int sortableBits, int pass, bool writeToFloat)
	{
		if (!writeToFloat)
			return;

		// Final pass converts sortable bits back to IEEE floats; middle passes store raw sortable bits.
		int bits = pass == SegmentedRadixPasses - 1 ? SortableToFloatBits(sortableBits) : sortableBits;
		floatView[index] = Interop.IntAsFloat((uint)bits);
	}

	static void WriteIntSortableKey(ArrayView<int> intView, int index, int sortableBits) =>
		intView[index] = sortableBits;

	/// <summary>
	/// One grid group per matrix row; in-place float sort with sortable-int radix passes.
	/// Pass 0 reads floats and writes sortable ints to temp; middle passes alias the float buffer
	/// as raw int bit patterns; the final pass writes sorted IEEE floats back in place.
	/// </summary>
	static void SegmentedSortFloatRowKern<TOperation>(
		ArrayView<float> input,
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
			bool readFromFloat = (pass & 1) == 0;
			bool writeToFloat = !readFromFloat;

			// Histogram: count digit occurrences for this pass's byte.
			for (int bucket = Group.IdxX; bucket < SegmentedRadixBuckets; bucket += Group.DimX)
				histogram[bucket] = 0;
			Group.Barrier();

			for (int i = Group.IdxX; i < cols; i += Group.DimX)
			{
				int srcIdx = rowStart + i;
				int value = readFromFloat
					? ReadFloatSortableKey(input, srcIdx, pass)
					: ReadIntSortableKey(temp, srcIdx);
				int digit = operation.ExtractRadixBits(value, shift, SegmentedRadixBuckets - 1);
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
				int value = readFromFloat
					? ReadFloatSortableKey(input, srcIdx, pass)
					: ReadIntSortableKey(temp, srcIdx);
				int digit = operation.ExtractRadixBits(value, shift, SegmentedRadixBuckets - 1);
				int dstIdx = rowStart + Atomic.Add(ref histogram[digit], 1);

				if (writeToFloat)
					WriteFloatSortableKey(input, dstIdx, value, pass, writeToFloat: true);
				else
					WriteIntSortableKey(temp, dstIdx, value);
			}
			Group.Barrier();
		}
	}

	/// <summary>
	/// Argsort variant: pass 0 reads unchanged float keys and writes sortable ints into <paramref name="keys"/>;
	/// remaining passes match the int pairs kernel. Caller must supply a writable keys buffer.
	/// </summary>
	static void SegmentedSortFloatPairsRowKern<TOperation>(
		ArrayView<float> floatInput,
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
				int srcIdx = rowStart + i;
				int key = pass == 0
					? FloatBitsToSortable((int)Interop.FloatAsInt(floatInput[srcIdx]))
					: readKeys[srcIdx];
				int digit = operation.ExtractRadixBits(key, shift, SegmentedRadixBuckets - 1);
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
				int key = pass == 0
					? FloatBitsToSortable((int)Interop.FloatAsInt(floatInput[srcIdx]))
					: readKeys[srcIdx];
				int value = readValues[srcIdx];
				int digit = operation.ExtractRadixBits(key, shift, SegmentedRadixBuckets - 1);
				int dstIdx = rowStart + Atomic.Add(ref histogram[digit], 1);
				writeKeys[dstIdx] = key;
				writeValues[dstIdx] = value;
			}
			Group.Barrier();
		}
	}
}
