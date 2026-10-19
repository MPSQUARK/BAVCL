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
		Guard.IsRowVector1D(vector);
		Guard.IsVector2D(matrix);

		if (vector.Length != matrix.Columns)
		{
			throw new LengthMismatchException(
				operationName,
				matrix.Columns,
				vector.Length);
		}
	}

	static void ValidateReduceOperands(VectorInt vector, VectorInt matrix, string operationName)
	{
		Guard.IsRowVector1D(vector);
		Guard.IsVector2D(matrix);

		if (vector.Length != matrix.Columns)
		{
			throw new LengthMismatchException(
				operationName,
				matrix.Columns,
				vector.Length);
		}
	}

}
