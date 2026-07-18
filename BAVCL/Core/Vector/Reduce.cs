using BAVCL.Core.Exceptions;

namespace BAVCL;

public partial class Vector
{
	public static Vector ReduceOP(Vector vector, Vector matrix, Operations operation)
	{
		ValidateReduceOperands(vector, matrix, nameof(ReduceOP));
		return RunReduceRowOp(vector, matrix, operation);
	}

	public Vector ReduceOP(Vector matrix, Operations operation) =>
		ReduceOP(this, matrix, operation);

	static void ValidateReduceOperands(Vector vector, Vector matrix, string operationName)
	{
		if (!vector.Is1D())
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
}
