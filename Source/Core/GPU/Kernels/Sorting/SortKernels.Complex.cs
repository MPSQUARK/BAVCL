using System;
using System.Numerics;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<long>> complexMagnitudeKey
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(complexMagnitudeKey), KernelDomain.Sorting);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<long>> complexRealKey
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(complexRealKey), KernelDomain.Sorting);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<long>> complexImaginaryKey
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(complexImaginaryKey), KernelDomain.Sorting);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<int>, ArrayView<long>, int> complexIndexedRealKey
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexIndexedRealKey), KernelDomain.Sorting);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>> complexGatherByIndex
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexGatherByIndex), KernelDomain.Sorting);

	public Action<AcceleratorStream, KernelConfig, ArrayView<long>, ArrayView<Complex>, ArrayView<long>, ArrayView<Complex>, int, int> segmentedSortComplexPairsAscIP
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortComplexPairsAscIP), KernelDomain.Sorting);

	public Action<AcceleratorStream, KernelConfig, ArrayView<long>, ArrayView<Complex>, ArrayView<long>, ArrayView<Complex>, int, int> segmentedSortComplexPairsDescIP
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedSortComplexPairsDescIP), KernelDomain.Sorting);

	public Action<AcceleratorStream, KernelConfig, ArrayView<long>, ArrayView<int>, ArrayView<long>, ArrayView<int>, int, int, int> segmentedArgsortComplexPairsAscIP
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedArgsortComplexPairsAscIP), KernelDomain.Sorting);

	public Action<AcceleratorStream, KernelConfig, ArrayView<long>, ArrayView<int>, ArrayView<long>, ArrayView<int>, int, int, int> segmentedArgsortComplexPairsDescIP
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(segmentedArgsortComplexPairsDescIP), KernelDomain.Sorting);

	internal void LoadSortComplexKernels()
	{
		complexMagnitudeKey = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<Complex>, ArrayView<long>>(ComplexMagnitudeKey_Kern);
		complexRealKey = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<Complex>, ArrayView<long>>(ComplexRealKey_Kern);
		complexImaginaryKey = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<Complex>, ArrayView<long>>(ComplexImaginaryKey_Kern);
		complexIndexedRealKey = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<Complex>, ArrayView<int>, ArrayView<long>, int>(ComplexIndexedRealKey_Kern);
		complexGatherByIndex = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>>(ComplexGatherByIndex_Kern);

		segmentedSortComplexPairsAscIP = accelerator.LoadKernel<ArrayView<long>, ArrayView<Complex>, ArrayView<long>, ArrayView<Complex>, int, int>(
			SegmentedSortComplexPairs_Kern<AscendingInt64>);
		segmentedSortComplexPairsDescIP = accelerator.LoadKernel<ArrayView<long>, ArrayView<Complex>, ArrayView<long>, ArrayView<Complex>, int, int>(
			SegmentedSortComplexPairs_Kern<DescendingInt64>);
		segmentedArgsortComplexPairsAscIP = accelerator.LoadKernel<ArrayView<long>, ArrayView<int>, ArrayView<long>, ArrayView<int>, int, int, int>(
			SegmentedArgsortComplexPairs_Kern<AscendingInt64>);
		segmentedArgsortComplexPairsDescIP = accelerator.LoadKernel<ArrayView<long>, ArrayView<int>, ArrayView<long>, ArrayView<int>, int, int, int>(
			SegmentedArgsortComplexPairs_Kern<DescendingInt64>);
	}

	static void ComplexMagnitudeKey_Kern(Index1D index, ArrayView<Complex> input, ArrayView<long> keys)
	{
		Complex value = input[index];
		long realBits = (long)Interop.FloatAsInt(value.Real) & long.MaxValue;
		long imaginaryBits = (long)Interop.FloatAsInt(value.Imaginary) & long.MaxValue;
		long realGreater = (imaginaryBits - realBits) >> 63;
		long maxBits = imaginaryBits ^ ((realBits ^ imaginaryBits) & realGreater);
		long minBits = realBits ^ ((imaginaryBits ^ realBits) & realGreater);
		double max = Interop.IntAsFloat((ulong)maxBits);
		double min = Interop.IntAsFloat((ulong)minBits);
		double scaled = max * XMath.Sqrt(1.0 + (min / max) * (min / max));
		long zero = ~((maxBits | -maxBits) >> 63);
		long scaledBits = (long)Interop.FloatAsInt(scaled);
		keys[index] = SortableDouble(Interop.IntAsFloat((ulong)(scaledBits & ~zero)));
	}

	static void ComplexRealKey_Kern(Index1D index, ArrayView<Complex> input, ArrayView<long> keys) =>
		keys[index] = SortableDouble(input[index].Real);

	static void ComplexImaginaryKey_Kern(Index1D index, ArrayView<Complex> input, ArrayView<long> keys) =>
		keys[index] = SortableDouble(input[index].Imaginary);

	static void ComplexIndexedRealKey_Kern(
		Index1D index, ArrayView<Complex> input, ArrayView<int> indices, ArrayView<long> keys, int columns)
	{
		int row = index / columns;
		keys[index] = SortableDouble(input[(row * columns) + indices[index]].Real);
	}

	static void ComplexGatherByIndex_Kern(Index1D index, ArrayView<Complex> output, ArrayView<Complex> input, ArrayView<int> indices) =>
		output[index] = input[indices[index]];

	/// <summary>
	/// One group per row. Keys are precomputed sortable component bits, so each pass is the float
	/// segmented radix with a 64-bit key: eight byte passes, no per-element branch. An even pass
	/// count writes the sorted pairs back into <paramref name="keys"/> and <paramref name="values"/>.
	/// </summary>
	static void SegmentedSortComplexPairs_Kern<TOperation>(
		ArrayView<long> keys,
		ArrayView<Complex> values,
		ArrayView<long> tempKeys,
		ArrayView<Complex> tempValues,
		int cols,
		int rowCount)
		where TOperation : struct, IRadixSortOperation<long>
	{
		int rowStart = Grid.IdxX * cols;
		TOperation operation = default;
		var histogram = SharedMemory.Allocate<int>(SegmentedRadixBuckets);
		var bucketStarts = SharedMemory.Allocate<int>(SegmentedRadixBuckets);

		for (int pass = 0; pass < ComplexRadixPasses; pass++)
		{
			int shift = pass << 3;
			bool readOriginal = (pass & 1) == 0;
			ArrayView<long> readKeys = Utilities.Select(readOriginal, keys, tempKeys);
			ArrayView<long> writeKeys = Utilities.Select(readOriginal, tempKeys, keys);
			ArrayView<Complex> readValues = Utilities.Select(readOriginal, values, tempValues);
			ArrayView<Complex> writeValues = Utilities.Select(readOriginal, tempValues, values);

			for (int bucket = Group.IdxX; bucket < SegmentedRadixBuckets; bucket += Group.DimX)
				histogram[bucket] = 0;
			Group.Barrier();

			for (int lane = Group.IdxX; lane < cols; lane += Group.DimX)
			{
				int digit = operation.ExtractRadixBits(readKeys[rowStart + lane], shift, SegmentedRadixBuckets - 1);
				Atomic.Add(ref histogram[digit], 1);
			}
			Group.Barrier();

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

			for (int lane = Group.IdxX; lane < cols; lane += Group.DimX)
			{
				int source = rowStart + lane;
				long key = readKeys[source];
				Complex value = readValues[source];
				int digit = operation.ExtractRadixBits(key, shift, SegmentedRadixBuckets - 1);
				int destination = rowStart + Atomic.Add(ref histogram[digit], 1);
				writeKeys[destination] = key;
				writeValues[destination] = value;
			}
			Group.Barrier();
		}
	}

	/// <summary>
	/// Argsort form of <see cref="SegmentedSortComplexPairs_Kern{TOperation}"/>.
	/// <paramref name="seed"/> is uniform: the first pass writes row-local indices, a later pass keeps them.
	/// </summary>
	static void SegmentedArgsortComplexPairs_Kern<TOperation>(
		ArrayView<long> keys,
		ArrayView<int> indices,
		ArrayView<long> tempKeys,
		ArrayView<int> tempIndices,
		int cols,
		int rowCount,
		int seed)
		where TOperation : struct, IRadixSortOperation<long>
	{
		int rowStart = Grid.IdxX * cols;
		TOperation operation = default;
		var histogram = SharedMemory.Allocate<int>(SegmentedRadixBuckets);
		var bucketStarts = SharedMemory.Allocate<int>(SegmentedRadixBuckets);

		if (seed != 0)
		{
			for (int lane = Group.IdxX; lane < cols; lane += Group.DimX)
				indices[rowStart + lane] = lane;
			Group.Barrier();
		}

		for (int pass = 0; pass < ComplexRadixPasses; pass++)
		{
			int shift = pass << 3;
			bool readOriginal = (pass & 1) == 0;

			ArrayView<long> readKeys = Utilities.Select(readOriginal, keys, tempKeys);
			ArrayView<long> writeKeys = Utilities.Select(readOriginal, tempKeys, keys);
			ArrayView<int> readIndices = Utilities.Select(readOriginal, indices, tempIndices);
			ArrayView<int> writeIndices = Utilities.Select(readOriginal, tempIndices, indices);

			for (int bucket = Group.IdxX; bucket < SegmentedRadixBuckets; bucket += Group.DimX)
				histogram[bucket] = 0;
			Group.Barrier();

			for (int lane = Group.IdxX; lane < cols; lane += Group.DimX)
			{
				int digit = operation.ExtractRadixBits(readKeys[rowStart + lane], shift, SegmentedRadixBuckets - 1);
				Atomic.Add(ref histogram[digit], 1);
			}
			Group.Barrier();

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

			for (int lane = Group.IdxX; lane < cols; lane += Group.DimX)
			{
				int source = rowStart + lane;
				long key = readKeys[source];
				int index = readIndices[source];
				int digit = operation.ExtractRadixBits(key, shift, SegmentedRadixBuckets - 1);
				int destination = rowStart + Atomic.Add(ref histogram[digit], 1);
				writeKeys[destination] = key;
				writeIndices[destination] = index;
			}
			Group.Barrier();
		}
	}

	const int ComplexRadixPasses = 8;

	/// <summary>Same mapping as <c>FloatBitsToSortable</c>, widened to a double.</summary>
	internal static long SortableDouble(double value)
	{
		long bits = (long)Interop.FloatAsInt(value);
		return bits ^ ((bits >> 63) & long.MaxValue);
	}
}
