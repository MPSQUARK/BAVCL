using System.Threading;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using BAVCL.GpuAlgorithms.ScanReduceOperations;

namespace BAVCL.GpuAlgorithms;

/// <summary>
/// Caches ILGPU.Algorithms scan and radix-sort delegates per GPU (one instance per variant).
/// Each delegate is compiled on first access. Temporary storage is sized and rented per invocation by callers.
/// </summary>
internal sealed class AlgorithmProviders
{
	private readonly Accelerator _accelerator;

	readonly Lock _syncIntSumInclusive = new();
	internal Scan<int, Stride1D.Dense, Stride1D.Dense> IntSumInclusive =>
		GetCompiledScan<int, AddInt32>(ref field, _syncIntSumInclusive, ScanKind.Inclusive);

	readonly Lock _syncIntSumExclusive = new();
	internal Scan<int, Stride1D.Dense, Stride1D.Dense> IntSumExclusive =>
		GetCompiledScan<int, AddInt32>(ref field, _syncIntSumExclusive, ScanKind.Exclusive);

	readonly Lock _syncIntProductInclusive = new();
	internal Scan<int, Stride1D.Dense, Stride1D.Dense> IntProductInclusive =>
		GetCompiledScan<int, MulInt32>(ref field, _syncIntProductInclusive, ScanKind.Inclusive);

	readonly Lock _syncIntProductExclusive = new();
	internal Scan<int, Stride1D.Dense, Stride1D.Dense> IntProductExclusive =>
		GetCompiledScan<int, MulInt32>(ref field, _syncIntProductExclusive, ScanKind.Exclusive);

	readonly Lock _syncFloatSumInclusive = new();
	internal Scan<float, Stride1D.Dense, Stride1D.Dense> FloatSumInclusive =>
		GetCompiledScan<float, AddFloat>(ref field, _syncFloatSumInclusive, ScanKind.Inclusive);

	readonly Lock _syncFloatSumExclusive = new();
	internal Scan<float, Stride1D.Dense, Stride1D.Dense> FloatSumExclusive =>
		GetCompiledScan<float, AddFloat>(ref field, _syncFloatSumExclusive, ScanKind.Exclusive);

	readonly Lock _syncFloatProductInclusive = new();
	internal Scan<float, Stride1D.Dense, Stride1D.Dense> FloatProductInclusive =>
		GetCompiledScan<float, MulFloat>(ref field, _syncFloatProductInclusive, ScanKind.Inclusive);

	readonly Lock _syncFloatProductExclusive = new();
	internal Scan<float, Stride1D.Dense, Stride1D.Dense> FloatProductExclusive =>
		GetCompiledScan<float, MulFloat>(ref field, _syncFloatProductExclusive, ScanKind.Exclusive);

	readonly Lock _syncAscendingIntSort = new();
	internal RadixSort<int, Stride1D.Dense> AscendingIntSort =>
		GetCompiledRadixSort<AscendingInt32>(ref field, _syncAscendingIntSort);

	readonly Lock _syncDescendingIntSort = new();
	internal RadixSort<int, Stride1D.Dense> DescendingIntSort =>
		GetCompiledRadixSort<DescendingInt32>(ref field, _syncDescendingIntSort);

	readonly Lock _syncAscendingIntPairsSort = new();
	internal RadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense> AscendingIntPairsSort =>
		GetCompiledRadixSortPairs<AscendingInt32>(ref field, _syncAscendingIntPairsSort);

	readonly Lock _syncDescendingIntPairsSort = new();
	internal RadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense> DescendingIntPairsSort =>
		GetCompiledRadixSortPairs<DescendingInt32>(ref field, _syncDescendingIntPairsSort);

	internal AlgorithmProviders(Accelerator accelerator) => _accelerator = accelerator;

	Scan<T, Stride1D.Dense, Stride1D.Dense> GetCompiledScan<T, TScanOp>(
		ref Scan<T, Stride1D.Dense, Stride1D.Dense>? registration,
		Lock sync,
		ScanKind kind)
		where T : unmanaged
		where TScanOp : struct, IScanReduceOperation<T>
	{
		if (Volatile.Read(ref registration) is not null)
			return registration!;

		lock (sync)
		{
			registration ??= _accelerator.CreateScan<T, Stride1D.Dense, Stride1D.Dense, TScanOp>(kind);
			return registration!;
		}
	}

	RadixSort<int, Stride1D.Dense> GetCompiledRadixSort<TOperation>(
		ref RadixSort<int, Stride1D.Dense>? registration,
		Lock sync)
		where TOperation : struct, IRadixSortOperation<int>
	{
		if (Volatile.Read(ref registration) is not null)
			return registration!;

		lock (sync)
		{
			registration ??= _accelerator.CreateRadixSort<int, Stride1D.Dense, TOperation>();
			return registration!;
		}
	}

	RadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense> GetCompiledRadixSortPairs<TOperation>(
		ref RadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense>? registration,
		Lock sync)
		where TOperation : struct, IRadixSortOperation<int>
	{
		if (Volatile.Read(ref registration) is not null)
			return registration!;

		lock (sync)
		{
			registration ??=
				_accelerator.CreateRadixSortPairs<int, Stride1D.Dense, int, Stride1D.Dense, TOperation>();
			return registration!;
		}
	}
}
