using BAVCL.Core.Exceptions;

namespace BAVCL;

public partial class Vector
{
	public static Vector MatrixAdd(Vector matrixA, Vector matrixB) =>
		MatrixBinary(matrixA, matrixB, Operations.add, nameof(MatrixAdd));

	public static Vector MatrixSubtract(Vector matrixA, Vector matrixB) =>
		MatrixBinary(matrixA, matrixB, Operations.subtract, nameof(MatrixSubtract));

	public static Vector MatrixDivide(Vector matrixA, Vector matrixB) =>
		MatrixBinary(matrixA, matrixB, Operations.divide, nameof(MatrixDivide));

	public static Vector MatrixPow(Vector matrixA, Vector matrixB) =>
		MatrixBinary(matrixA, matrixB, Operations.pow, nameof(MatrixPow));

	public static Vector MatrixMultiply(Vector matrixA, Vector matrixB) =>
		Cross(matrixA, matrixB);

	public Vector MatrixAdd(Vector matrixB) => MatrixAdd(this, matrixB);
	public Vector MatrixSubtract(Vector matrixB) => MatrixSubtract(this, matrixB);
	public Vector MatrixDivide(Vector matrixB) => MatrixDivide(this, matrixB);
	public Vector MatrixPow(Vector matrixB) => MatrixPow(this, matrixB);
	public Vector MatrixMultiply(Vector matrixB) => MatrixMultiply(this, matrixB);

	static Vector MatrixBinary(Vector matrixA, Vector matrixB, Operations operation, string operationName)
	{
		EnsureMatrix2D(matrixA, operationName);
		EnsureMatrix2D(matrixB, operationName);

		(int rowsA, int colsA) = matrixA.Shape();
		(int rowsB, int colsB) = matrixB.Shape();

		if (rowsA != rowsB || colsA != colsB)
		{
			throw new ShapeMismatchException(
				operationName,
				$"({rowsA},{colsA})",
				$"({rowsB},{colsB})");
		}

		return _VectorVectorOP(matrixA, matrixB, operation);
	}

	static void EnsureMatrix2D(Vector vector, string operationName)
	{
		if (vector.Columns <= 1)
		{
			(int rows, int cols) = vector.Shape();
			throw new ShapeMismatchException(
				operationName,
				$"({rows},{cols})",
				"(requires 2D matrix)");
		}
	}
}
