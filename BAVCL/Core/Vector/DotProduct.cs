using BAVCL.Core.Exceptions;

namespace BAVCL;

public partial class Vector
{
	public static float Dot(Vector vectorA, Vector vectorB)
	{
		if (vectorA.Length != vectorB.Length)
			throw new LengthMismatchException(nameof(Dot), vectorA.Length, vectorB.Length);

		return OP(vectorA, vectorB, Operations.multiply).Sum();
	}

	public static float Dot(Vector vectorA, float scalar) =>
		OP(vectorA, scalar, Operations.multiply).Sum();

	public float Dot(Vector vectorB) => Dot(this, vectorB);

	public float Dot(float scalar) =>
		OP(this, scalar, Operations.multiply).Sum();
}
