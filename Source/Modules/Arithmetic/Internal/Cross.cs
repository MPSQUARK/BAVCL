using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;
using BAVCL.Modules.GpuOps;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Arithmetic;

/// <summary>
/// Matrix multiply dispatch for <see cref="Vector"/> (BAVCL "Cross", not geometric cross product).
/// </summary>
internal static class CrossCore
{
	internal static Vector Cross(Vector left, Vector right)
	{
		if (left.RowCount() == 1 && right.Columns > 1 && left.Columns == right.RowCount())
			return CrossMatMul2D(left, right);

		if (IsMatMul2D(left, right))
			return CrossMatMul2D(left, right);

		if (left.Is1D() && right.Columns > 1)
			return CrossVectorMatrix(left, right);

		if (left.Columns > 1 && (right.Is1D() || right.Columns == 1))
			return CrossMatrixVector(left, right);

		throw new ArgumentException(
			$"Cross requires 2D×2D, 1D×2D, or 2D×1D operands. Got shapes ({left.RowCount()},{left.Columns}) and ({right.RowCount()},{right.Columns}).");
	}

	static bool IsMatMul2D(Vector a, Vector b) => a.Columns > 1 && b.Columns > 1;

	internal static Vector CrossMatMul2D(Vector matrixA, Vector matrixB)
	{
		int rowsA = matrixA.RowCount();
		int colsA = matrixA.Columns;
		int rowsB = matrixB.RowCount();
		int colsB = matrixB.Columns;

		if (colsA != rowsB)
		{
			throw new ArgumentException(
				$"Cross inner dimensions must match: ({rowsA},{colsA}) × ({rowsB},{colsB}).");
		}

		GPU gpu = matrixA.Gpu;
		Vector output = new(gpu, rowsA * colsB, colsB);

		using (GpuScope.Begin(output, matrixA, matrixB))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				bufferA = matrixA.GetBuffer(),
				bufferB = matrixB.GetBuffer();

			gpu.matmulKernel(
				gpu.accelerator.DefaultStream,
				rowsA,
				buffer.View,
				bufferA.View,
				bufferB.View,
				colsA,
				colsB);

			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector CrossVectorMatrix(Vector vector, Vector matrix)
	{
		if (vector.Length != matrix.Columns)
		{
			throw new LengthMismatchException(
				"Cross 1D×2D",
				matrix.Columns,
				vector.Length);
		}

		return VectorVectorOp.RunReduceRowOp(vector, matrix, Operations.multiply);
	}

	internal static Vector CrossMatrixVector(Vector matrix, Vector vector)
	{
		if (matrix.Columns != vector.Length)
		{
			throw new LengthMismatchException(
				"Cross 2D×1D",
				matrix.Columns,
				vector.Length);
		}

		return VectorVectorOp.RunReduceRowOp(vector, matrix, Operations.multiply);
	}

	internal static VectorInt Cross(VectorInt left, VectorInt right)
	{
		if (left.RowCount() == 1 && right.Columns > 1 && left.Columns == right.RowCount())
			return CrossMatMul2D(left, right);

		if (IsMatMul2D(left, right))
			return CrossMatMul2D(left, right);

		if (left.Is1D() && right.Columns > 1)
			return CrossVectorMatrix(left, right);

		if (left.Columns > 1 && (right.Is1D() || right.Columns == 1))
			return CrossMatrixVector(left, right);

		throw new ArgumentException(
			$"Cross requires 2D×2D, 1D×2D, or 2D×1D operands. Got shapes ({left.RowCount()},{left.Columns}) and ({right.RowCount()},{right.Columns}).");
	}

	static bool IsMatMul2D(VectorInt a, VectorInt b) => a.Columns > 1 && b.Columns > 1;

	internal static VectorInt CrossMatMul2D(VectorInt matrixA, VectorInt matrixB)
	{
		int rowsA = matrixA.RowCount();
		int colsA = matrixA.Columns;
		int rowsB = matrixB.RowCount();
		int colsB = matrixB.Columns;

		if (colsA != rowsB)
		{
			throw new ArgumentException(
				$"Cross inner dimensions must match: ({rowsA},{colsA}) × ({rowsB},{colsB}).");
		}

		GPU gpu = matrixA.Gpu;
		VectorInt output = new(gpu, rowsA * colsB, colsB);

		using (GpuScope.Begin(output, matrixA, matrixB))
		{
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer = output.GetBuffer(),
				bufferA = matrixA.GetBuffer(),
				bufferB = matrixB.GetBuffer();

			gpu.matmulIntKernel(
				gpu.DefaultStream,
				rowsA,
				buffer.View,
				bufferA.View,
				bufferB.View,
				colsA,
				colsB);
			gpu.Synchronize();
		}

		return output;
	}

	internal static VectorInt CrossVectorMatrix(VectorInt vector, VectorInt matrix)
	{
		if (vector.Length != matrix.Columns)
		{
			throw new LengthMismatchException(
				"Cross 1D×2D",
				matrix.Columns,
				vector.Length);
		}

		return VectorVectorOp.RunReduceRowOp(vector, matrix, Operations.multiply);
	}

	internal static VectorInt CrossMatrixVector(VectorInt matrix, VectorInt vector)
	{
		if (matrix.Columns != vector.Length)
		{
			throw new LengthMismatchException(
				"Cross 2D×1D",
				matrix.Columns,
				vector.Length);
		}

		return VectorVectorOp.RunReduceRowOp(vector, matrix, Operations.multiply);
	}
}
