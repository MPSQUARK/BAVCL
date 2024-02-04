namespace BAVCL;

public partial class Vector
{
	public static Vector Normalise(Vector vectorA) =>
		OP(vectorA, 1f / vectorA.Sum(), Operations.multiply);

	public Vector Normalise_IP() =>
		IPOP(1f / Sum(), Operations.multiply);

}