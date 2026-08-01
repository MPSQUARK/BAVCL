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

	internal static VectorInt MatrixAdd(VectorInt left, VectorInt right) =>
		MatrixBinary(left, right, Operations.add, nameof(MatrixAdd));

	internal static VectorInt MatrixSubtract(VectorInt left, VectorInt right) =>
		MatrixBinary(left, right, Operations.subtract, nameof(MatrixSubtract));

	internal static VectorInt MatrixDivide(VectorInt left, VectorInt right) =>
		MatrixBinary(left, right, Operations.divide, nameof(MatrixDivide));

	internal static VectorInt MatrixMultiply(VectorInt left, VectorInt right) =>
		CrossCore.Cross(left, right);

	static VectorInt MatrixBinary(VectorInt matrixA, VectorInt matrixB, Operations operation, string operationName)
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

	static void EnsureMatrix2D(VectorInt vector, string operationName)
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
