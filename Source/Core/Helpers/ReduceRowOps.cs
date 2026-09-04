using System;

namespace BAVCL.Core.Helpers;

/// <summary>Host-side row-reduce element ops. Device kernels mirror this in <c>GPU.ReduceRowElement*</c>.</summary>
internal static class ReduceRowOps
{
	internal static float ElementFloat(float coeff, float matrixValue, Operations operation) =>
		operation switch
		{
			Operations.multiply => coeff * matrixValue,
			Operations.add => coeff + matrixValue,
			Operations.subtract => coeff - matrixValue,
			Operations.flipSubtract => matrixValue - coeff,
			Operations.divide => coeff / matrixValue,
			Operations.flipDivide => matrixValue / coeff,
			Operations.pow => MathF.Pow(coeff, matrixValue),
			Operations.flipPow => MathF.Pow(matrixValue, coeff),
			Operations.differenceSquared => MathF.Pow(coeff - matrixValue, 2f),
			Operations.distance => MathF.Pow(coeff - matrixValue, 2f),
			_ => 0f,
		};

	internal static int ElementInt(int coeff, int matrixValue, Operations operation) =>
		operation switch
		{
			Operations.multiply => coeff * matrixValue,
			Operations.add => coeff + matrixValue,
			Operations.subtract => coeff - matrixValue,
			Operations.flipSubtract => matrixValue - coeff,
			Operations.divide => coeff / matrixValue,
			Operations.flipDivide => matrixValue / coeff,
			_ => 0,
		};

	internal static bool UsesDistance(Operations operation) => operation == Operations.distance;
}
