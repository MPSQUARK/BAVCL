using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using BAVCL.Core.Memory;

namespace BAVCL.GpuAlgorithms;

internal static class ScanAlgorithms
{
	internal static Vector CumSumInclusiveX(Vector vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.FloatSumInclusive);

	internal static VectorInt CumSumInclusiveX(VectorInt vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.IntSumInclusive);

	internal static Vector CumSumExclusiveX(Vector vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.FloatSumExclusive);

	internal static VectorInt CumSumExclusiveX(VectorInt vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.IntSumExclusive);

	internal static Vector CumProdInclusiveX(Vector vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.FloatProductInclusive);

	internal static VectorInt CumProdInclusiveX(VectorInt vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.IntProductInclusive);

	internal static Vector CumProdExclusiveX(Vector vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.FloatProductExclusive);

	internal static VectorInt CumProdExclusiveX(VectorInt vector) =>
		RunScanAllocating(vector, vector.Gpu.AlgorithmProviders.IntProductExclusive);

	internal static void CumSumInclusiveXIP(Vector vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.FloatSumInclusive);

	internal static void CumSumInclusiveXIP(VectorInt vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.IntSumInclusive);

	internal static void CumSumExclusiveXIP(Vector vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.FloatSumExclusive);

	internal static void CumSumExclusiveXIP(VectorInt vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.IntSumExclusive);

	internal static void CumProdInclusiveXIP(Vector vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.FloatProductInclusive);

	internal static void CumProdInclusiveXIP(VectorInt vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.IntProductInclusive);

	internal static void CumProdExclusiveXIP(Vector vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.FloatProductExclusive);

	internal static void CumProdExclusiveXIP(VectorInt vector) =>
		RunScanInPlace(vector, vector.Gpu.AlgorithmProviders.IntProductExclusive);

	static Vector RunScanAllocating(Vector vector, Scan<float, Stride1D.Dense, Stride1D.Dense> scan)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Vector output = vector.Copy();
		RunScanInPlace(output, scan);
		return output;
	}

	static VectorInt RunScanAllocating(VectorInt vector, Scan<int, Stride1D.Dense, Stride1D.Dense> scan)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		VectorInt output = vector.Copy();
		RunScanInPlace(output, scan);
		return output;
	}

	static void RunScanInPlace<T, TVector>(TVector vector, Scan<T, Stride1D.Dense, Stride1D.Dense> scan)
		where T : unmanaged
		where TVector : VectorBase<T>
	{
		if (vector.Length == 0)
			return;

		GPU gpu = vector.Gpu;
		if (vector.Is2D())
			RunScanInPlace2D(gpu, vector, scan);
		else
			RunScanInPlace1D(gpu, vector, scan);
	}

	static void RunScanInPlace1D<T, TVector>(
		GPU gpu,
		TVector vector,
		Scan<T, Stride1D.Dense, Stride1D.Dense> scan)
		where T : unmanaged
		where TVector : VectorBase<T>
	{
		int tempElements = gpu.accelerator.ComputeScanTempStorageSize<T>(vector.Length).ToIntIndex();
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> temp = BufferPools.For(gpu).Int.Rent(tempElements);
			using (GpuScope.Begin(temp.PinTarget))
			{
				MemoryBuffer1D<T, Stride1D.Dense> buffer = vector.GetBuffer();
				scan(gpu.DefaultStream, buffer.View, buffer.View, temp.View);
			}

			gpu.Synchronize();
		}
	}

	static void RunScanInPlace2D<T, TVector>(
		GPU gpu,
		TVector vector,
		Scan<T, Stride1D.Dense, Stride1D.Dense> scan)
		where T : unmanaged
		where TVector : VectorBase<T>
	{
		Shape shape = vector.Shape();
		int cols = shape.Cols;
		int tempElements = gpu.accelerator.ComputeScanTempStorageSize<T>(cols).ToIntIndex();
		using (GpuScope.Begin(vector))
		{
			using BufferEntity<int> temp = BufferPools.For(gpu).Int.Rent(tempElements);
			using (GpuScope.Begin(temp.PinTarget))
			{
				ArrayView1D<T, Stride1D.Dense> view = vector.GetBuffer().View;
				for (int row = 0, offset = 0; row < shape.Rows; row++, offset += cols)
					scan(gpu.DefaultStream, view.SubView(offset, cols), view.SubView(offset, cols), temp.View);
			}

			gpu.Synchronize();
		}
	}
}
