using System;
using System.Threading;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU.Runtime;

namespace BAVCL.GpuAlgorithms;

/// <summary>
/// Caches ILGPU.Algorithms radix-sort providers, and their compiled sort delegates, per GPU.
/// Both the provider (temp storage) and the delegate (compiled kernel handle) are only
/// (re)created when the requested capacity exceeds what is cached.
/// </summary>
internal sealed class AlgorithmProviders : IDisposable
{
	private readonly Accelerator _accelerator;
	private readonly Lock _lock = new();

	private RadixSortProvider? _ascendingIntProvider;
	private long _ascendingIntCapacity;
	private BufferedRadixSort<int, Stride1D.Dense>? _ascendingIntSort;

	private RadixSortProvider? _descendingIntProvider;
	private long _descendingIntCapacity;
	private BufferedRadixSort<int, Stride1D.Dense>? _descendingIntSort;

	private RadixSortProvider? _ascendingIntPairsProvider;
	private long _ascendingIntPairsCapacity;
	private BufferedRadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense>? _ascendingIntPairsSort;

	private RadixSortProvider? _descendingIntPairsProvider;
	private long _descendingIntPairsCapacity;
	private BufferedRadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense>? _descendingIntPairsSort;

	internal AlgorithmProviders(Accelerator accelerator) => _accelerator = accelerator;

	internal BufferedRadixSort<int, Stride1D.Dense> GetAscendingIntSort(long segmentLength) =>
		GetOrCreate(
			ref _ascendingIntProvider, ref _ascendingIntCapacity, ref _ascendingIntSort, segmentLength,
			length => _accelerator.CreateRadixSortProvider<int, AscendingInt32>(GPU.SortLaunchExtent(length)),
			provider => provider.CreateRadixSort<int, Stride1D.Dense, AscendingInt32>());

	internal BufferedRadixSort<int, Stride1D.Dense> GetDescendingIntSort(long segmentLength) =>
		GetOrCreate(
			ref _descendingIntProvider, ref _descendingIntCapacity, ref _descendingIntSort, segmentLength,
			length => _accelerator.CreateRadixSortProvider<int, DescendingInt32>(GPU.SortLaunchExtent(length)),
			provider => provider.CreateRadixSort<int, Stride1D.Dense, DescendingInt32>());

	internal BufferedRadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense> GetAscendingIntPairsSort(long segmentLength) =>
		GetOrCreate(
			ref _ascendingIntPairsProvider, ref _ascendingIntPairsCapacity, ref _ascendingIntPairsSort, segmentLength,
			length => _accelerator.CreateRadixSortProvider<int, int, AscendingInt32>(GPU.SortLaunchExtent(length)),
			provider => provider.CreateRadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense, AscendingInt32>());

	internal BufferedRadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense> GetDescendingIntPairsSort(long segmentLength) =>
		GetOrCreate(
			ref _descendingIntPairsProvider, ref _descendingIntPairsCapacity, ref _descendingIntPairsSort, segmentLength,
			length => _accelerator.CreateRadixSortProvider<int, int, DescendingInt32>(GPU.SortLaunchExtent(length)),
			provider => provider.CreateRadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense, DescendingInt32>());

	TDelegate GetOrCreate<TDelegate>(
		ref RadixSortProvider? provider,
		ref long capacity,
		ref TDelegate? cachedSort,
		long segmentLength,
		Func<long, RadixSortProvider> createProvider,
		Func<RadixSortProvider, TDelegate> createSort) where TDelegate : class
	{
		lock (_lock)
		{
			if (provider is null || segmentLength > capacity)
			{
				provider?.Dispose();
				provider = createProvider(segmentLength);
				capacity = segmentLength;
				cachedSort = createSort(provider);
			}

			return cachedSort!;
		}
	}

	public void Dispose()
	{
		_ascendingIntProvider?.Dispose();
		_descendingIntProvider?.Dispose();
		_ascendingIntPairsProvider?.Dispose();
		_descendingIntPairsProvider?.Dispose();
	}
}
