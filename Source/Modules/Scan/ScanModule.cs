using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Scan;

public static class VectorScan
{
	extension(Vector)
	{
		public static Vector CumSum(Vector vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, syncToGpu)
				: ScanCore.CumSumInclusive1D(vector, syncToGpu);

		public static Vector CumSumExclusive(Vector vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, syncToGpu)
				: ScanCore.CumSumExclusive1D(vector, syncToGpu);

		public static Vector CumProd(Vector vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, syncToGpu)
				: ScanCore.CumProdInclusive1D(vector, syncToGpu);

		public static Vector CumProdExclusive(Vector vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, syncToGpu)
				: ScanCore.CumProdExclusive1D(vector, syncToGpu);

		public static void CumSumIP(Vector vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumInclusiveIP1D(vector, syncToGpu);
		}

		public static void CumSumExclusiveIP(Vector vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumExclusiveIP1D(vector, syncToGpu);
		}

		public static void CumProdIP(Vector vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdInclusiveIP1D(vector, syncToGpu);
		}

		public static void CumProdExclusiveIP(Vector vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdExclusiveIP1D(vector, syncToGpu);
		}

		public static Vector CumSumX(Vector vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumSumInclusiveX2D(vector)
				: ScanAlgorithms.CumSumInclusiveX1D(vector);

		public static Vector CumSumExclusiveX(Vector vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumSumExclusiveX2D(vector)
				: ScanAlgorithms.CumSumExclusiveX1D(vector);

		public static Vector CumProdX(Vector vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumProdInclusiveX2D(vector)
				: ScanAlgorithms.CumProdInclusiveX1D(vector);

		public static Vector CumProdExclusiveX(Vector vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumProdExclusiveX2D(vector)
				: ScanAlgorithms.CumProdExclusiveX1D(vector);

		public static void CumSumXIP(Vector vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumInclusiveXIP1D(vector);
		}

		public static void CumSumExclusiveXIP(Vector vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumExclusiveXIP1D(vector);
		}

		public static void CumProdXIP(Vector vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdInclusiveXIP1D(vector);
		}

		public static void CumProdExclusiveXIP(Vector vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdExclusiveXIP1D(vector);
		}
	}

	extension(VectorInt)
	{
		public static VectorInt CumSum(VectorInt vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, syncToGpu)
				: ScanCore.CumSumInclusive1D(vector, syncToGpu);

		public static VectorInt CumSumExclusive(VectorInt vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, syncToGpu)
				: ScanCore.CumSumExclusive1D(vector, syncToGpu);

		public static VectorInt CumProd(VectorInt vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, syncToGpu)
				: ScanCore.CumProdInclusive1D(vector, syncToGpu);

		public static VectorInt CumProdExclusive(VectorInt vector, bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, syncToGpu)
				: ScanCore.CumProdExclusive1D(vector, syncToGpu);

		public static void CumSumIP(VectorInt vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumInclusiveIP1D(vector, syncToGpu);
		}

		public static void CumSumExclusiveIP(VectorInt vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumExclusiveIP1D(vector, syncToGpu);
		}

		public static void CumProdIP(VectorInt vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdInclusiveIP1D(vector, syncToGpu);
		}

		public static void CumProdExclusiveIP(VectorInt vector, bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdExclusiveIP1D(vector, syncToGpu);
		}

		public static VectorInt CumSumX(VectorInt vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumSumInclusiveX2D(vector)
				: ScanAlgorithms.CumSumInclusiveX1D(vector);

		public static VectorInt CumSumExclusiveX(VectorInt vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumSumExclusiveX2D(vector)
				: ScanAlgorithms.CumSumExclusiveX1D(vector);

		public static VectorInt CumProdX(VectorInt vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumProdInclusiveX2D(vector)
				: ScanAlgorithms.CumProdInclusiveX1D(vector);

		public static VectorInt CumProdExclusiveX(VectorInt vector) =>
			vector.Is2D()
				? ScanAlgorithms.CumProdExclusiveX2D(vector)
				: ScanAlgorithms.CumProdExclusiveX1D(vector);

		public static void CumSumXIP(VectorInt vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumInclusiveXIP1D(vector);
		}

		public static void CumSumExclusiveXIP(VectorInt vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumExclusiveXIP1D(vector);
		}

		public static void CumProdXIP(VectorInt vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdInclusiveXIP1D(vector);
		}

		public static void CumProdExclusiveXIP(VectorInt vector)
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdExclusiveXIP1D(vector);
		}
	}
}

public static class ScanModuleExtensions
{
	extension(Vector vector)
	{
		public Vector CumSum(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, syncToGpu)
				: ScanCore.CumSumInclusive1D(vector, syncToGpu);

		public Vector CumSumExclusive(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, syncToGpu)
				: ScanCore.CumSumExclusive1D(vector, syncToGpu);

		public Vector CumProd(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, syncToGpu)
				: ScanCore.CumProdInclusive1D(vector, syncToGpu);

		public Vector CumProdExclusive(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, syncToGpu)
				: ScanCore.CumProdExclusive1D(vector, syncToGpu);

		public void CumSumIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumInclusiveIP1D(vector, syncToGpu);
		}

		public void CumSumExclusiveIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumExclusiveIP1D(vector, syncToGpu);
		}

		public void CumProdIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdInclusiveIP1D(vector, syncToGpu);
		}

		public void CumProdExclusiveIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdExclusiveIP1D(vector, syncToGpu);
		}

		public Vector CumSumX() =>
			vector.Is2D()
				? ScanAlgorithms.CumSumInclusiveX2D(vector)
				: ScanAlgorithms.CumSumInclusiveX1D(vector);

		public Vector CumSumExclusiveX() =>
			vector.Is2D()
				? ScanAlgorithms.CumSumExclusiveX2D(vector)
				: ScanAlgorithms.CumSumExclusiveX1D(vector);

		public Vector CumProdX() =>
			vector.Is2D()
				? ScanAlgorithms.CumProdInclusiveX2D(vector)
				: ScanAlgorithms.CumProdInclusiveX1D(vector);

		public Vector CumProdExclusiveX() =>
			vector.Is2D()
				? ScanAlgorithms.CumProdExclusiveX2D(vector)
				: ScanAlgorithms.CumProdExclusiveX1D(vector);

		public void CumSumXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumInclusiveXIP1D(vector);
		}

		public void CumSumExclusiveXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumExclusiveXIP1D(vector);
		}

		public void CumProdXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdInclusiveXIP1D(vector);
		}

		public void CumProdExclusiveXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdExclusiveXIP1D(vector);
		}
	}

	extension(VectorInt vector)
	{
		public VectorInt CumSum(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, syncToGpu)
				: ScanCore.CumSumInclusive1D(vector, syncToGpu);

		public VectorInt CumSumExclusive(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, syncToGpu)
				: ScanCore.CumSumExclusive1D(vector, syncToGpu);

		public VectorInt CumProd(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, syncToGpu)
				: ScanCore.CumProdInclusive1D(vector, syncToGpu);

		public VectorInt CumProdExclusive(bool syncToGpu = true) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, syncToGpu)
				: ScanCore.CumProdExclusive1D(vector, syncToGpu);

		public void CumSumIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumInclusiveIP1D(vector, syncToGpu);
		}

		public void CumSumExclusiveIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumSumExclusiveIP1D(vector, syncToGpu);
		}

		public void CumProdIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdInclusiveIP1D(vector, syncToGpu);
		}

		public void CumProdExclusiveIP(bool syncToGpu = true)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, syncToGpu);
			else
				ScanCore.CumProdExclusiveIP1D(vector, syncToGpu);
		}

		public VectorInt CumSumX() =>
			vector.Is2D()
				? ScanAlgorithms.CumSumInclusiveX2D(vector)
				: ScanAlgorithms.CumSumInclusiveX1D(vector);

		public VectorInt CumSumExclusiveX() =>
			vector.Is2D()
				? ScanAlgorithms.CumSumExclusiveX2D(vector)
				: ScanAlgorithms.CumSumExclusiveX1D(vector);

		public VectorInt CumProdX() =>
			vector.Is2D()
				? ScanAlgorithms.CumProdInclusiveX2D(vector)
				: ScanAlgorithms.CumProdInclusiveX1D(vector);

		public VectorInt CumProdExclusiveX() =>
			vector.Is2D()
				? ScanAlgorithms.CumProdExclusiveX2D(vector)
				: ScanAlgorithms.CumProdExclusiveX1D(vector);

		public void CumSumXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumInclusiveXIP1D(vector);
		}

		public void CumSumExclusiveXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumSumExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumSumExclusiveXIP1D(vector);
		}

		public void CumProdXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdInclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdInclusiveXIP1D(vector);
		}

		public void CumProdExclusiveXIP()
		{
			if (vector.Is2D())
				ScanAlgorithms.CumProdExclusiveXIP2D(vector);
			else
				ScanAlgorithms.CumProdExclusiveXIP1D(vector);
		}
	}
}
