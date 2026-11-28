using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Scan;

public static class VectorScan
{
	extension(Vector)
	{
		public static Vector CumSum(Vector vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, upload)
				: ScanCore.CumSumInclusive1D(vector, upload);

		public static Vector CumSumExclusive(Vector vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, upload)
				: ScanCore.CumSumExclusive1D(vector, upload);

		public static Vector CumProd(Vector vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, upload)
				: ScanCore.CumProdInclusive1D(vector, upload);

		public static Vector CumProdExclusive(Vector vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, upload)
				: ScanCore.CumProdExclusive1D(vector, upload);

		public static void CumSumIP(Vector vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumInclusiveIP1D(vector, upload);
		}

		public static void CumSumExclusiveIP(Vector vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumExclusiveIP1D(vector, upload);
		}

		public static void CumProdIP(Vector vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdInclusiveIP1D(vector, upload);
		}

		public static void CumProdExclusiveIP(Vector vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdExclusiveIP1D(vector, upload);
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
		public static VectorInt CumSum(VectorInt vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, upload)
				: ScanCore.CumSumInclusive1D(vector, upload);

		public static VectorInt CumSumExclusive(VectorInt vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, upload)
				: ScanCore.CumSumExclusive1D(vector, upload);

		public static VectorInt CumProd(VectorInt vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, upload)
				: ScanCore.CumProdInclusive1D(vector, upload);

		public static VectorInt CumProdExclusive(VectorInt vector, Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, upload)
				: ScanCore.CumProdExclusive1D(vector, upload);

		public static void CumSumIP(VectorInt vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumInclusiveIP1D(vector, upload);
		}

		public static void CumSumExclusiveIP(VectorInt vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumExclusiveIP1D(vector, upload);
		}

		public static void CumProdIP(VectorInt vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdInclusiveIP1D(vector, upload);
		}

		public static void CumProdExclusiveIP(VectorInt vector, Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdExclusiveIP1D(vector, upload);
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
		public Vector CumSum(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, upload)
				: ScanCore.CumSumInclusive1D(vector, upload);

		public Vector CumSumExclusive(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, upload)
				: ScanCore.CumSumExclusive1D(vector, upload);

		public Vector CumProd(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, upload)
				: ScanCore.CumProdInclusive1D(vector, upload);

		public Vector CumProdExclusive(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, upload)
				: ScanCore.CumProdExclusive1D(vector, upload);

		public void CumSumIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumInclusiveIP1D(vector, upload);
		}

		public void CumSumExclusiveIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumExclusiveIP1D(vector, upload);
		}

		public void CumProdIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdInclusiveIP1D(vector, upload);
		}

		public void CumProdExclusiveIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdExclusiveIP1D(vector, upload);
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
		public VectorInt CumSum(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumInclusive2D(vector, upload)
				: ScanCore.CumSumInclusive1D(vector, upload);

		public VectorInt CumSumExclusive(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumSumExclusive2D(vector, upload)
				: ScanCore.CumSumExclusive1D(vector, upload);

		public VectorInt CumProd(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdInclusive2D(vector, upload)
				: ScanCore.CumProdInclusive1D(vector, upload);

		public VectorInt CumProdExclusive(Upload upload = Upload.Now) =>
			vector.Is2D()
				? ScanCore.CumProdExclusive2D(vector, upload)
				: ScanCore.CumProdExclusive1D(vector, upload);

		public void CumSumIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumInclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumInclusiveIP1D(vector, upload);
		}

		public void CumSumExclusiveIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumSumExclusiveIP2D(vector, upload);
			else
				ScanCore.CumSumExclusiveIP1D(vector, upload);
		}

		public void CumProdIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdInclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdInclusiveIP1D(vector, upload);
		}

		public void CumProdExclusiveIP(Upload upload = Upload.Now)
		{
			if (vector.Is2D())
				ScanCore.CumProdExclusiveIP2D(vector, upload);
			else
				ScanCore.CumProdExclusiveIP1D(vector, upload);
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
