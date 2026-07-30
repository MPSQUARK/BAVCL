using BAVCL.Core.Exceptions;
using BAVCL.Modules.GpuOps;

namespace BAVCL.Modules.Arithmetic;

internal static class MatrixOpsCore
{
	internal static Vector MatrixAdd(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.add, nameof(MatrixAdd));

	internal static Vector MatrixSubtract(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.subtract, nameof(MatrixSubtract));

	internal static Vector MatrixDivide(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.divide, nameof(MatrixDivide));

	internal static Vector MatrixPow(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.pow, nameof(MatrixPow));

	internal static Vector MatrixMultiply(Vector left, Vector right) =>
		CrossCore.Cross(left, right);

	static Vector MatrixBinary(Vector matrixA, Vector matrixB, Operations operation, string operationName)
	{
		EnsureMatrix2D(matrixA, operationName);
		EnsureMatrix2D(matrixB, operationName);

		Shape shapeA = matrixA.Shape();
		Shape shapeB = matrixB.Shape();

		if (shapeA.Rows != shapeB.Rows || shapeA.Cols != shapeB.Cols)
		{
			throw new ShapeMismatchException(
				operationName,
				shapeA.ToString(),
				shapeB.ToString());
		}

		return VectorVectorOp.VectorVectorOP(matrixA, matrixB, operation);
	}

	static void EnsureMatrix2D(Vector vector, string operationName)
	{
		if (vector.Columns <= 1)
		{
			Shape shape = vector.Shape();
			throw new ShapeMismatchException(
				operationName,
				shape.ToString(),
				"(requires 2D matrix)");
		}
	}
}
