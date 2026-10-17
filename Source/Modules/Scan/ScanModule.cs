using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Scan;

public static class VectorScan
{
	extension(Vector)
	{
		public static Vector CumSum(Vector vector, bool syncToGpu = true) => ScanCore.CumSumInclusive(vector, syncToGpu);

		public static Vector CumSumExclusive(Vector vector, bool syncToGpu = true) => ScanCore.CumSumExclusive(vector, syncToGpu);

		public static Vector CumProd(Vector vector, bool syncToGpu = true) => ScanCore.CumProdInclusive(vector, syncToGpu);

		public static Vector CumProdExclusive(Vector vector, bool syncToGpu = true) => ScanCore.CumProdExclusive(vector, syncToGpu);

		public static void CumSumIP(Vector vector, bool syncToGpu = true) => ScanCore.CumSumInclusiveIP(vector, syncToGpu);

		public static void CumSumExclusiveIP(Vector vector, bool syncToGpu = true) => ScanCore.CumSumExclusiveIP(vector, syncToGpu);

		public static void CumProdIP(Vector vector, bool syncToGpu = true) => ScanCore.CumProdInclusiveIP(vector, syncToGpu);

		public static void CumProdExclusiveIP(Vector vector, bool syncToGpu = true) => ScanCore.CumProdExclusiveIP(vector, syncToGpu);

		public static Vector CumSumX(Vector vector) => ScanAlgorithms.CumSumInclusiveX(vector);

		public static Vector CumSumExclusiveX(Vector vector) => ScanAlgorithms.CumSumExclusiveX(vector);

		public static Vector CumProdX(Vector vector) => ScanAlgorithms.CumProdInclusiveX(vector);

		public static Vector CumProdExclusiveX(Vector vector) => ScanAlgorithms.CumProdExclusiveX(vector);

		public static void CumSumXIP(Vector vector) => ScanAlgorithms.CumSumInclusiveXIP(vector);

		public static void CumSumExclusiveXIP(Vector vector) => ScanAlgorithms.CumSumExclusiveXIP(vector);

		public static void CumProdXIP(Vector vector) => ScanAlgorithms.CumProdInclusiveXIP(vector);

		public static void CumProdExclusiveXIP(Vector vector) => ScanAlgorithms.CumProdExclusiveXIP(vector);
	}

	extension(VectorInt)
	{
		public static VectorInt CumSum(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSumInclusive(vector, syncToGpu);

		public static VectorInt CumSumExclusive(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSumExclusive(vector, syncToGpu);

		public static VectorInt CumProd(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProdInclusive(vector, syncToGpu);

		public static VectorInt CumProdExclusive(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProdExclusive(vector, syncToGpu);

		public static void CumSumIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSumInclusiveIP(vector, syncToGpu);

		public static void CumSumExclusiveIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSumExclusiveIP(vector, syncToGpu);

		public static void CumProdIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProdInclusiveIP(vector, syncToGpu);

		public static void CumProdExclusiveIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProdExclusiveIP(vector, syncToGpu);

		public static VectorInt CumSumX(VectorInt vector) => ScanAlgorithms.CumSumInclusiveX(vector);

		public static VectorInt CumSumExclusiveX(VectorInt vector) => ScanAlgorithms.CumSumExclusiveX(vector);

		public static VectorInt CumProdX(VectorInt vector) => ScanAlgorithms.CumProdInclusiveX(vector);

		public static VectorInt CumProdExclusiveX(VectorInt vector) => ScanAlgorithms.CumProdExclusiveX(vector);

		public static void CumSumXIP(VectorInt vector) => ScanAlgorithms.CumSumInclusiveXIP(vector);

		public static void CumSumExclusiveXIP(VectorInt vector) => ScanAlgorithms.CumSumExclusiveXIP(vector);

		public static void CumProdXIP(VectorInt vector) => ScanAlgorithms.CumProdInclusiveXIP(vector);

		public static void CumProdExclusiveXIP(VectorInt vector) => ScanAlgorithms.CumProdExclusiveXIP(vector);
	}
}

public static class ScanModuleExtensions
{
	extension(Vector vector)
	{
		public Vector CumSum(bool syncToGpu = true) => ScanCore.CumSumInclusive(vector, syncToGpu);

		public Vector CumSumExclusive(bool syncToGpu = true) => ScanCore.CumSumExclusive(vector, syncToGpu);

		public Vector CumProd(bool syncToGpu = true) => ScanCore.CumProdInclusive(vector, syncToGpu);

		public Vector CumProdExclusive(bool syncToGpu = true) => ScanCore.CumProdExclusive(vector, syncToGpu);

		public void CumSumIP(bool syncToGpu = true) => ScanCore.CumSumInclusiveIP(vector, syncToGpu);

		public void CumSumExclusiveIP(bool syncToGpu = true) => ScanCore.CumSumExclusiveIP(vector, syncToGpu);

		public void CumProdIP(bool syncToGpu = true) => ScanCore.CumProdInclusiveIP(vector, syncToGpu);

		public void CumProdExclusiveIP(bool syncToGpu = true) => ScanCore.CumProdExclusiveIP(vector, syncToGpu);

		public Vector CumSumX() => ScanAlgorithms.CumSumInclusiveX(vector);

		public Vector CumSumExclusiveX() => ScanAlgorithms.CumSumExclusiveX(vector);

		public Vector CumProdX() => ScanAlgorithms.CumProdInclusiveX(vector);

		public Vector CumProdExclusiveX() => ScanAlgorithms.CumProdExclusiveX(vector);

		public void CumSumXIP() => ScanAlgorithms.CumSumInclusiveXIP(vector);

		public void CumSumExclusiveXIP() => ScanAlgorithms.CumSumExclusiveXIP(vector);

		public void CumProdXIP() => ScanAlgorithms.CumProdInclusiveXIP(vector);

		public void CumProdExclusiveXIP() => ScanAlgorithms.CumProdExclusiveXIP(vector);
	}

	extension(VectorInt vector)
	{
		public VectorInt CumSum(bool syncToGpu = true) => ScanCore.CumSumInclusive(vector, syncToGpu);

		public VectorInt CumSumExclusive(bool syncToGpu = true) => ScanCore.CumSumExclusive(vector, syncToGpu);

		public VectorInt CumProd(bool syncToGpu = true) => ScanCore.CumProdInclusive(vector, syncToGpu);

		public VectorInt CumProdExclusive(bool syncToGpu = true) => ScanCore.CumProdExclusive(vector, syncToGpu);

		public void CumSumIP(bool syncToGpu = true) => ScanCore.CumSumInclusiveIP(vector, syncToGpu);

		public void CumSumExclusiveIP(bool syncToGpu = true) => ScanCore.CumSumExclusiveIP(vector, syncToGpu);

		public void CumProdIP(bool syncToGpu = true) => ScanCore.CumProdInclusiveIP(vector, syncToGpu);

		public void CumProdExclusiveIP(bool syncToGpu = true) => ScanCore.CumProdExclusiveIP(vector, syncToGpu);

		public VectorInt CumSumX() => ScanAlgorithms.CumSumInclusiveX(vector);

		public VectorInt CumSumExclusiveX() => ScanAlgorithms.CumSumExclusiveX(vector);

		public VectorInt CumProdX() => ScanAlgorithms.CumProdInclusiveX(vector);

		public VectorInt CumProdExclusiveX() => ScanAlgorithms.CumProdExclusiveX(vector);

		public void CumSumXIP() => ScanAlgorithms.CumSumInclusiveXIP(vector);

		public void CumSumExclusiveXIP() => ScanAlgorithms.CumSumExclusiveXIP(vector);

		public void CumProdXIP() => ScanAlgorithms.CumProdInclusiveXIP(vector);

		public void CumProdExclusiveXIP() => ScanAlgorithms.CumProdExclusiveXIP(vector);
	}
}
