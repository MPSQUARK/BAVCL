using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{
	/// <summary>
	/// Matrix multiply (Cross): (M×K) × (K×N) → (M×N) for 2D operands;
	/// 1D×2D and 2D×1D use vector-matrix multiply (row dot products).
	/// Geometric 3D cross product is <see cref="Geometric.Vector3.Cross"/>.
	/// </summary>
	public static Vector Cross(Vector vectorA, Vector vectorB)
	{
		if (vectorA.RowCount() == 1 && vectorB.Columns > 1 && vectorA.Columns == vectorB.RowCount())
			return CrossMatMul2D(vectorA, vectorB);

		if (IsMatMul2D(vectorA, vectorB))
			return CrossMatMul2D(vectorA, vectorB);

		if (vectorA.Is1D() && vectorB.Columns > 1)
			return CrossVectorMatrix(vectorA, vectorB);

		if (vectorA.Columns > 1 && (vectorB.Is1D() || vectorB.Columns == 1))
			return CrossMatrixVector(vectorA, vectorB);

		throw new ArgumentException(
			$"Cross requires 2D×2D, 1D×2D, or 2D×1D operands. Got shapes ({vectorA.RowCount()},{vectorA.Columns}) and ({vectorB.RowCount()},{vectorB.Columns}).");
	}

	public Vector Cross(Vector vectorB) => Cross(this, vectorB);

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

		return RunReduceRowOp(vector, matrix, Operations.multiply);
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

		return RunReduceRowOp(vector, matrix, Operations.multiply);
	}
}
