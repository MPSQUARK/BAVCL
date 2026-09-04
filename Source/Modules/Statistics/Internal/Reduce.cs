using BAVCL.Core.Exceptions;
using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Statistics;

internal static class ReduceCore
{
	internal static Vector ReduceOPX(Vector vector, Vector matrix, Operations operation)
	{
		ValidateReduceOperands(vector, matrix, nameof(ReduceOPX));
		return RowReduceAlgorithms.Reduce(vector, matrix, operation);
	}

	internal static VectorInt ReduceOPX(VectorInt vector, VectorInt matrix, Operations operation)
	{
		ValidateReduceOperands(vector, matrix, nameof(ReduceOPX));
		return RowReduceAlgorithms.Reduce(vector, matrix, operation);
	}

	static void ValidateReduceOperands(Vector vector, Vector matrix, string operationName)
	{
		if (!vector.Is1DRowVector())
		{
			throw new ShapeMismatchException(
				operationName,
				FormatShape(vector),
				"(requires 1D row coefficient vector, Columns=0)");
		}

		if (matrix.Columns <= 1)
		{
			throw new ShapeMismatchException(
				operationName,
				FormatShape(matrix),
				"(requires 2D matrix, Columns > 1)");
		}

		if (vector.Length != matrix.Columns)
		{
			throw new LengthMismatchException(
				operationName,
				matrix.Columns,
				vector.Length);
		}
	}

	static string FormatShape(Vector vector) => vector.Shape().ToString();

	static void ValidateReduceOperands(VectorInt vector, VectorInt matrix, string operationName)
	{
		if (!vector.Is1DRowVector())
		{
			throw new ShapeMismatchException(
				operationName,
				FormatShape(vector),
				"(requires 1D row coefficient vector, Columns=0)");
		}

		if (matrix.Columns <= 1)
		{
			throw new ShapeMismatchException(
				operationName,
				FormatShape(matrix),
				"(requires 2D matrix, Columns > 1)");
		}

		if (vector.Length != matrix.Columns)
		{
			throw new LengthMismatchException(
				operationName,
				matrix.Columns,
				vector.Length);
		}
	}

	static string FormatShape(VectorInt vector) => vector.Shape().ToString();
}
