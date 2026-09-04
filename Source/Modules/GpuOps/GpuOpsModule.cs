using BAVCL.Geometric;

namespace BAVCL.Modules.GpuOps;

public static class GpuOpsModule
{
	extension(Vector)
	{
		public static Vector OP(Vector left, Vector right, Operations operation) =>
			Broadcast.BroadcastOP(left, right, operation);

		public static Vector OP(Vector vector, float scalar, Operations operation) =>
			VectorGpuOps.ScalarOP(vector, scalar, operation);

		public static Vector IPOP(Vector vector, Vector right, Operations operation) =>
			Broadcast.BroadcastIPOP(vector, right, operation);

		public static Vector IPOP(Vector vector, float scalar, Operations operation) =>
			VectorGpuOps.ScalarIPOP(vector, scalar, operation);

		public static Vector LogX(Vector vector, float @base) =>
			VectorGpuOps.LogX(vector, @base);

		public static Vector LogXIP(Vector vector, float @base) =>
			VectorGpuOps.LogXIP(vector, @base);
	}

	extension(VectorInt)
	{
		public static VectorInt OP(VectorInt left, VectorInt right, Operations operation) =>
			Broadcast.BroadcastOP(left, right, operation);

		public static VectorInt OP(VectorInt vector, int scalar, Operations operation) =>
			VectorIntGpuOps.ScalarOP(vector, scalar, operation);

		public static VectorInt IPOP(VectorInt vector, VectorInt right, Operations operation) =>
			Broadcast.BroadcastIPOP(vector, right, operation);

		public static VectorInt IPOP(VectorInt vector, int scalar, Operations operation) =>
			VectorIntGpuOps.ScalarIPOP(vector, scalar, operation);
	}

	extension(Vector3)
	{
		public static Vector VOPX(Vector3 vector, Operations operation) =>
			Vector3GpuOps.VOPX(vector, operation);

		public static Vector VOPX(Vector3 left, Vector3 right, Operations operation) =>
			Vector3GpuOps.VOPX(left, right, operation);

		public static Vector3 OPX(Vector3 left, Vector3 right, Operations operation) =>
			Vector3GpuOps.OPX(left, right, operation);

		public static Vector3 OPX(Vector3 vector, float scalar, Operations operation) =>
			Vector3GpuOps.OPX(vector, scalar, operation);

		public static Vector3 IPOP(Vector3 vector, Vector3 other, Operations operation) =>
			Vector3GpuOps.IPOP(vector, other, operation);

		public static Vector3 IPOP(Vector3 vector, float scalar, Operations operation) =>
			Vector3GpuOps.IPOP(vector, scalar, operation);
	}
}

public static class GpuOpsModuleExtensions
{
	extension(Vector vectorA)
	{
		public Vector OP(Vector vectorB, Operations operation) =>
			Vector.OP(vectorA, vectorB, operation);

		public Vector IPOP(Vector vectorB, Operations operation) =>
			Vector.IPOP(vectorA, vectorB, operation);

		public Vector OP(float scalar, Operations operation) =>
			Vector.OP(vectorA, scalar, operation);

		public Vector IPOP(float scalar, Operations operation) =>
			Vector.IPOP(vectorA, scalar, operation);

		public Vector LogX(float @base) =>
			Vector.LogX(vectorA, @base);

		public Vector LogXIP(float @base) =>
			Vector.LogXIP(vectorA, @base);
	}

	extension(VectorInt vectorA)
	{
		public VectorInt OP(VectorInt vectorB, Operations operation) =>
			VectorInt.OP(vectorA, vectorB, operation);

		public VectorInt IPOP(VectorInt vectorB, Operations operation) =>
			VectorInt.IPOP(vectorA, vectorB, operation);

		public VectorInt OP(int scalar, Operations operation) =>
			VectorInt.OP(vectorA, scalar, operation);

		public VectorInt IPOP(int scalar, Operations operation) =>
			VectorInt.IPOP(vectorA, scalar, operation);
	}

	extension(Vector3 vectorA)
	{
		public Vector VOPX(Operations operation) =>
			Vector3.VOPX(vectorA, operation);

		public Vector VOPX(Vector3 vectorB, Operations operation) =>
			Vector3.VOPX(vectorA, vectorB, operation);

		public Vector3 OPX(Vector3 vectorB, Operations operation) =>
			Vector3.OPX(vectorA, vectorB, operation);

		public Vector3 OPX(float scalar, Operations operation) =>
			Vector3.OPX(vectorA, scalar, operation);

		public Vector3 IPOP(Vector3 other, Operations operation) =>
			Vector3.IPOP(vectorA, other, operation);

		public Vector3 IPOP(float scalar, Operations operation) =>
			Vector3.IPOP(vectorA, scalar, operation);
	}
}
