using BAVCL.Modules.GpuOps;

namespace BAVCL.Geometric;

public partial class Vector3
{
	public static Vector3 operator +(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.add);

	public static Vector3 operator -(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.subtract);

	public static Vector3 operator *(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.multiply);

	public static Vector3 operator /(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.divide);

	public static Vector3 operator ^(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.pow);

	public static Vector3 operator +(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.add);

	public static Vector3 operator -(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.subtract);

	public static Vector3 operator *(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.multiply);

	public static Vector3 operator /(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.divide);

	public static Vector3 operator ^(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.pow);

	public static Vector3 operator +(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.add);

	public static Vector3 operator -(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.flipSubtract);

	public static Vector3 operator *(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.multiply);

	public static Vector3 operator /(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.flipDivide);

	public static Vector3 operator ^(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.flipPow);
}
