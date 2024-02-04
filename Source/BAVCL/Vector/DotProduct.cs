namespace BAVCL;

public partial class Vector
{
	public static float Dot(Vector vectorA, Vector vectorB) =>
		OP(vectorA, vectorB, Operations.multiply).Sum();

	public static float Dot(Vector vectorA, float scalar) =>
		OP(vectorA, scalar, Operations.multiply).Sum();

	public float Dot(Vector vectorB) =>
		OP(this, vectorB, Operations.multiply).Sum();

	public float Dot(float scalar) =>
		OP(this, scalar, Operations.multiply).Sum();

}