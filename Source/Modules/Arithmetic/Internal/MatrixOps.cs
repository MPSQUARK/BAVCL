using BAVCL.Core.Exceptions;
using BAVCL.Modules.GpuOps;

namespace BAVCL.Modules.Arithmetic;

internal static class MatrixOpsCore
{
	internal static Vector MatrixAddX(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.add, nameof(MatrixAddX));

	internal static Vector MatrixSubtractX(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.subtract, nameof(MatrixSubtractX));

	internal static Vector MatrixDivideX(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.divide, nameof(MatrixDivideX));

	internal static Vector MatrixPowX(Vector left, Vector right) =>
		MatrixBinary(left, right, Operations.pow, nameof(MatrixPowX));

	internal static Vector MatrixMultiplyX(Vector left, Vector right) =>
		CrossCore.CrossX(left, right);

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

	internal static VectorInt MatrixAddX(VectorInt left, VectorInt right) =>
		MatrixBinary(left, right, Operations.add, nameof(MatrixAddX));

	internal static VectorInt MatrixSubtractX(VectorInt left, VectorInt right) =>
		MatrixBinary(left, right, Operations.subtract, nameof(MatrixSubtractX));

	internal static VectorInt MatrixDivideX(VectorInt left, VectorInt right) =>
		MatrixBinary(left, right, Operations.divide, nameof(MatrixDivideX));

	internal static VectorInt MatrixMultiplyX(VectorInt left, VectorInt right) =>
		CrossCore.CrossX(left, right);

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
