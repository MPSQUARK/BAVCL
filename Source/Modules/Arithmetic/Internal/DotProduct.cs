using BAVCL.Core.Exceptions;
using BAVCL.Modules.GpuOps;

namespace BAVCL.Modules.Arithmetic;

internal static class DotProductCore
{
	internal static float Dot(Vector left, Vector right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(Dot), left.Length, right.Length);

		return left.OP(right, Operations.multiply).Sum();
	}

	internal static float Dot(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.multiply).Sum();
}
