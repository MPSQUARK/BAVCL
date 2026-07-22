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
			Broadcast.BroadcastOP_IP(vector, right, operation);

		public static Vector IPOP(Vector vector, float scalar, Operations operation) =>
			VectorGpuOps.ScalarOP_IP(vector, scalar, operation);

		public static Vector Log_IP(Vector vector, float @base) =>
			VectorGpuOps.Log_IP(vector, @base);
	}

	extension(Vector3)
	{
		public static Vector VOP(Vector3 vector, Operations operation) =>
			Vector3GpuOps.VOP(vector, operation);

		public static Vector VOP(Vector3 left, Vector3 right, Operations operation) =>
			Vector3GpuOps.VOP(left, right, operation);

		public static Vector3 OP(Vector3 left, Vector3 right, Operations operation) =>
			Vector3GpuOps.OP(left, right, operation);

		public static Vector3 OP(Vector3 vector, float scalar, Operations operation) =>
			Vector3GpuOps.OP(vector, scalar, operation);

		public static Vector3 OP_IP(Vector3 vector, Vector3 other, Operations operation) =>
			Vector3GpuOps.OP_IP(vector, other, operation);

		public static Vector3 OP_IP(Vector3 vector, float scalar, Operations operation) =>
			Vector3GpuOps.OP_IP(vector, scalar, operation);
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

		public Vector Log_IP(float @base) =>
			Vector.Log_IP(vectorA, @base);
	}

	extension(Vector3 vectorA)
	{
		public Vector VOP(Operations operation) =>
			Vector3.VOP(vectorA, operation);

		public Vector VOP(Vector3 vectorB, Operations operation) =>
			Vector3.VOP(vectorA, vectorB, operation);

		public Vector3 OP(Vector3 vectorB, Operations operation) =>
			Vector3.OP(vectorA, vectorB, operation);

		public Vector3 OP(float scalar, Operations operation) =>
			Vector3.OP(vectorA, scalar, operation);

		public Vector3 OP_IP(Vector3 other, Operations operation) =>
			Vector3.OP_IP(vectorA, other, operation);

		public Vector3 OP_IP(float scalar, Operations operation) =>
			Vector3.OP_IP(vectorA, scalar, operation);
	}
}
