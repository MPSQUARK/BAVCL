using BAVCL.GpuAlgorithms;
using BAVCL.Types;
using ILGPU.Algorithms;

namespace BAVCL.Modules.Scan;

public static class VectorScan
{
	extension(Vector)
	{
		public static Vector CumSum(Vector vector, bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Inclusive, syncToGpu);

		public static Vector CumSumExclusive(Vector vector, bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Exclusive, syncToGpu);

		public static Vector CumProd(Vector vector, bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Inclusive, syncToGpu);

		public static Vector CumProdExclusive(Vector vector, bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Exclusive, syncToGpu);

		public static void CumSumIP(Vector vector, bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Inclusive, syncToGpu);

		public static void CumSumExclusiveIP(Vector vector, bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Exclusive, syncToGpu);

		public static void CumProdIP(Vector vector, bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Inclusive, syncToGpu);

		public static void CumProdExclusiveIP(Vector vector, bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Exclusive, syncToGpu);

		public static Vector CumSumX(Vector vector) => ScanAlgorithms.CumSumX(vector, ScanKind.Inclusive);

		public static Vector CumSumExclusiveX(Vector vector) => ScanAlgorithms.CumSumX(vector, ScanKind.Exclusive);

		public static Vector CumProdX(Vector vector) => ScanAlgorithms.CumProdX(vector, ScanKind.Inclusive);

		public static Vector CumProdExclusiveX(Vector vector) => ScanAlgorithms.CumProdX(vector, ScanKind.Exclusive);

		public static void CumSumXIP(Vector vector) => ScanAlgorithms.CumSumXIP(vector, ScanKind.Inclusive);

		public static void CumSumExclusiveXIP(Vector vector) => ScanAlgorithms.CumSumXIP(vector, ScanKind.Exclusive);

		public static void CumProdXIP(Vector vector) => ScanAlgorithms.CumProdXIP(vector, ScanKind.Inclusive);

		public static void CumProdExclusiveXIP(Vector vector) => ScanAlgorithms.CumProdXIP(vector, ScanKind.Exclusive);
	}

	extension(VectorInt)
	{
		public static VectorInt CumSum(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Inclusive, syncToGpu);

		public static VectorInt CumSumExclusive(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Exclusive, syncToGpu);

		public static VectorInt CumProd(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Inclusive, syncToGpu);

		public static VectorInt CumProdExclusive(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Exclusive, syncToGpu);

		public static void CumSumIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Inclusive, syncToGpu);

		public static void CumSumExclusiveIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Exclusive, syncToGpu);

		public static void CumProdIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Inclusive, syncToGpu);

		public static void CumProdExclusiveIP(VectorInt vector, bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Exclusive, syncToGpu);

		public static VectorInt CumSumX(VectorInt vector) => ScanAlgorithms.CumSumX(vector, ScanKind.Inclusive);

		public static VectorInt CumSumExclusiveX(VectorInt vector) => ScanAlgorithms.CumSumX(vector, ScanKind.Exclusive);

		public static VectorInt CumProdX(VectorInt vector) => ScanAlgorithms.CumProdX(vector, ScanKind.Inclusive);

		public static VectorInt CumProdExclusiveX(VectorInt vector) => ScanAlgorithms.CumProdX(vector, ScanKind.Exclusive);

		public static void CumSumXIP(VectorInt vector) => ScanAlgorithms.CumSumXIP(vector, ScanKind.Inclusive);

		public static void CumSumExclusiveXIP(VectorInt vector) => ScanAlgorithms.CumSumXIP(vector, ScanKind.Exclusive);

		public static void CumProdXIP(VectorInt vector) => ScanAlgorithms.CumProdXIP(vector, ScanKind.Inclusive);

		public static void CumProdExclusiveXIP(VectorInt vector) => ScanAlgorithms.CumProdXIP(vector, ScanKind.Exclusive);
	}
}

public static class ScanModuleExtensions
{
	extension(Vector vector)
	{
		public Vector CumSum(bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Inclusive, syncToGpu);

		public Vector CumSumExclusive(bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Exclusive, syncToGpu);

		public Vector CumProd(bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Inclusive, syncToGpu);

		public Vector CumProdExclusive(bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Exclusive, syncToGpu);

		public void CumSumIP(bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Inclusive, syncToGpu);

		public void CumSumExclusiveIP(bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Exclusive, syncToGpu);

		public void CumProdIP(bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Inclusive, syncToGpu);

		public void CumProdExclusiveIP(bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Exclusive, syncToGpu);

		public Vector CumSumX() => ScanAlgorithms.CumSumX(vector, ScanKind.Inclusive);

		public Vector CumSumExclusiveX() => ScanAlgorithms.CumSumX(vector, ScanKind.Exclusive);

		public Vector CumProdX() => ScanAlgorithms.CumProdX(vector, ScanKind.Inclusive);

		public Vector CumProdExclusiveX() => ScanAlgorithms.CumProdX(vector, ScanKind.Exclusive);

		public void CumSumXIP() => ScanAlgorithms.CumSumXIP(vector, ScanKind.Inclusive);

		public void CumSumExclusiveXIP() => ScanAlgorithms.CumSumXIP(vector, ScanKind.Exclusive);

		public void CumProdXIP() => ScanAlgorithms.CumProdXIP(vector, ScanKind.Inclusive);

		public void CumProdExclusiveXIP() => ScanAlgorithms.CumProdXIP(vector, ScanKind.Exclusive);
	}

	extension(VectorInt vector)
	{
		public VectorInt CumSum(bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Inclusive, syncToGpu);

		public VectorInt CumSumExclusive(bool syncToGpu = true) => ScanCore.CumSum(vector, ScanKind.Exclusive, syncToGpu);

		public VectorInt CumProd(bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Inclusive, syncToGpu);

		public VectorInt CumProdExclusive(bool syncToGpu = true) => ScanCore.CumProd(vector, ScanKind.Exclusive, syncToGpu);

		public void CumSumIP(bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Inclusive, syncToGpu);

		public void CumSumExclusiveIP(bool syncToGpu = true) => ScanCore.CumSumIP(vector, ScanKind.Exclusive, syncToGpu);

		public void CumProdIP(bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Inclusive, syncToGpu);

		public void CumProdExclusiveIP(bool syncToGpu = true) => ScanCore.CumProdIP(vector, ScanKind.Exclusive, syncToGpu);

		public VectorInt CumSumX() => ScanAlgorithms.CumSumX(vector, ScanKind.Inclusive);

		public VectorInt CumSumExclusiveX() => ScanAlgorithms.CumSumX(vector, ScanKind.Exclusive);

		public VectorInt CumProdX() => ScanAlgorithms.CumProdX(vector, ScanKind.Inclusive);

		public VectorInt CumProdExclusiveX() => ScanAlgorithms.CumProdX(vector, ScanKind.Exclusive);

		public void CumSumXIP() => ScanAlgorithms.CumSumXIP(vector, ScanKind.Inclusive);

		public void CumSumExclusiveXIP() => ScanAlgorithms.CumSumXIP(vector, ScanKind.Exclusive);

		public void CumProdXIP() => ScanAlgorithms.CumProdXIP(vector, ScanKind.Inclusive);

		public void CumProdExclusiveXIP() => ScanAlgorithms.CumProdXIP(vector, ScanKind.Exclusive);
	}
}
