using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using BAVCL.Core.Memory;

namespace BAVCL.GpuAlgorithms;

internal static class ScanAlgorithms
{
	internal static Vector CumSumInclusiveX1D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumInclusive;
		Vector output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static Vector CumSumInclusiveX2D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumInclusive;
		Vector output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumSumInclusiveX1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumInclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumSumInclusiveX2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumInclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static Vector CumSumExclusiveX1D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumExclusive;
		Vector output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static Vector CumSumExclusiveX2D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumExclusive;
		Vector output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumSumExclusiveX1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumExclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumSumExclusiveX2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumExclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static Vector CumProdInclusiveX1D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductInclusive;
		Vector output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static Vector CumProdInclusiveX2D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductInclusive;
		Vector output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumProdInclusiveX1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductInclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumProdInclusiveX2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductInclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static Vector CumProdExclusiveX1D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductExclusive;
		Vector output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static Vector CumProdExclusiveX2D(Vector vector)
	{
		if (vector.Length == 0)
			return new Vector(vector.Gpu, 0, vector.Columns);

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductExclusive;
		Vector output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumProdExclusiveX1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductExclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace1D(vector.Gpu, output, scan);
		return output;
	}

	internal static VectorInt CumProdExclusiveX2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductExclusive;
		VectorInt output = vector.Copy();
		RunScanInPlace2D(vector.Gpu, output, scan);
		return output;
	}

	internal static void CumSumInclusiveXIP1D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumInclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumSumInclusiveXIP2D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumInclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
	}

	internal static void CumSumInclusiveXIP1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumInclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumSumInclusiveXIP2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumInclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
	}

	internal static void CumSumExclusiveXIP1D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumExclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumSumExclusiveXIP2D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatSumExclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
	}

	internal static void CumSumExclusiveXIP1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumExclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumSumExclusiveXIP2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntSumExclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
	}

	internal static void CumProdInclusiveXIP1D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductInclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumProdInclusiveXIP2D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductInclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
	}

	internal static void CumProdInclusiveXIP1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductInclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumProdInclusiveXIP2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductInclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
	}

	internal static void CumProdExclusiveXIP1D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductExclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumProdExclusiveXIP2D(Vector vector)
	{
		if (vector.Length == 0)
			return;

		Scan<float, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.FloatProductExclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
	}

	internal static void CumProdExclusiveXIP1D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductExclusive;
		RunScanInPlace1D(vector.Gpu, vector, scan);
	}

	internal static void CumProdExclusiveXIP2D(VectorInt vector)
	{
		if (vector.Length == 0)
			return;

		Scan<int, Stride1D.Dense, Stride1D.Dense> scan = vector.Gpu.AlgorithmProviders.IntProductExclusive;
		RunScanInPlace2D(vector.Gpu, vector, scan);
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
