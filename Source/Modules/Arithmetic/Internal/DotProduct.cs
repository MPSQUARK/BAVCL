using System;
using BAVCL.Core.Exceptions;
using BAVCL.Modules.Statistics;

namespace BAVCL.Modules.Arithmetic;

internal static class DotProductCore
{
	internal static float Dot(Vector left, Vector right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(Dot), left.Length, right.Length);

		return CpuSimdReduce.DotFloat(left.RetrieveReadOnlySpan(), right.RetrieveReadOnlySpan());
	}

	internal static float Dot(Vector vector, float scalar) =>
		CpuSimdReduce.DotFloat(vector.RetrieveReadOnlySpan(), scalar);

	internal static float Dot(VectorInt left, VectorInt right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(Dot), left.Length, right.Length);

		return CpuSimdReduce.DotInt(left.RetrieveReadOnlySpan(), right.RetrieveReadOnlySpan());
	}

	internal static float Dot(VectorInt vector, int scalar) =>
		CpuSimdReduce.DotInt(vector.RetrieveReadOnlySpan(), scalar);
}
