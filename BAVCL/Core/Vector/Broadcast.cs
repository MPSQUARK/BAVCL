using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{
	/// <summary>
	/// NumPy-style allocating broadcast for <see cref="OP(Vector, Vector, Operations)"/> and operator overloads.
	/// Uses the element-wise kernel when shapes and storage match; otherwise <c>broadcastOpKernel</c>.
	/// </summary>
	internal static Vector _BroadcastOP(Vector vectorA, Vector vectorB, Operations operation)
	{
		(int rowsA, int colsA) = vectorA.Shape();
		(int rowsB, int colsB) = vectorB.Shape();
		EnsureBroadcastable(nameof(OP), rowsA, colsA, rowsB, colsB);

		if (rowsA == rowsB && colsA == colsB
			&& vectorA.Length == vectorB.Length
			&& vectorA.Columns == vectorB.Columns)
			return _VectorVectorOP(vectorA, vectorB, operation);

		return RunBroadcastOp(vectorA, vectorB, operation, rowsA, colsA, rowsB, colsB);
	}

	/// <summary>
	/// In-place broadcast for <see cref="IPOP(Vector, Operations)"/>.
	/// Left operand must already match the broadcast output shape; otherwise <see cref="PerformanceException"/>.
	/// </summary>
	internal Vector _BroadcastOP_IP(Vector vectorB, Operations operation)
	{
		(int leftRows, int leftCols) = Shape();
		(int rightRows, int rightCols) = vectorB.Shape();
		EnsureBroadcastable(nameof(IPOP), leftRows, leftCols, rightRows, rightCols);

		(int outRows, int outCols) = BroadcastShape(leftRows, leftCols, rightRows, rightCols);
		if (leftRows != outRows || leftCols != outCols)
		{
			throw new PerformanceException(
				"Swap operand order OR use allocating overload 'OP'.");
		}

		if (leftRows == rightRows && leftCols == rightCols
			&& Length == vectorB.Length
			&& Columns == vectorB.Columns)
			return _VectorVectorOP_IP(vectorB, operation);

		RunBroadcastOpIP(this, vectorB, operation, rightRows, rightCols);
		return this;
	}

	/// <summary>
	/// Validates that two logical shapes broadcast together; rethrows <see cref="ShapeMismatchException"/>
	/// with the calling operation name and operand shapes.
	/// </summary>
	static void EnsureBroadcastable(string operation, int rowsA, int colsA, int rowsB, int colsB)
	{
		try
		{
			BroadcastShape(rowsA, colsA, rowsB, colsB);
		}
		catch (ShapeMismatchException)
		{
			throw new ShapeMismatchException(
				operation,
				$"({rowsA},{colsA})",
				$"({rowsB},{colsB})");
		}
	}

	/// <summary>
	/// NumPy broadcast output shape: apply <see cref="BroadcastDim"/> independently to rows and columns.
	/// </summary>
	static (int rows, int cols) BroadcastShape(int rowsA, int colsA, int rowsB, int colsB) =>
		(BroadcastDim(rowsA, rowsB), BroadcastDim(colsA, colsB));

	/// <summary>
	/// NumPy rule for one axis: equal sizes match; size <c>1</c> broadcasts to the other; else incompatible.
	/// </summary>
	static int BroadcastDim(int a, int b)
	{
		if (a == b)
			return a;

		if (a == 1)
			return b;

		if (b == 1)
			return a;

		throw new ShapeMismatchException("broadcast", $"({a})", $"({b})");
	}

	/// <summary>
	/// Maps logical broadcast output shape to BAVCL <see cref="VectorBase{T}.Columns"/> storage:
	/// <c>(M,1)</c> → <c>1</c>, <c>(1,N)</c> → <c>0</c> (row 1D), <c>(M,N)</c> → <c>N</c>.
	/// </summary>
	static int StorageColumnsForShape(int outRows, int outCols)
	{
		if (outCols == 1)
			return 1;

		if (outRows == 1)
			return 0;

		return outCols;
	}

	/// <summary>
	/// Allocates the broadcast output vector and runs <c>broadcastOpKernel</c> with logical operand shapes.
	/// </summary>
	static Vector RunBroadcastOp(
		Vector vectorA,
		Vector vectorB,
		Operations operation,
		int rowsA,
		int colsA,
		int rowsB,
		int colsB)
	{
		GPU gpu = vectorA.Gpu;
		(int outRows, int outCols) = BroadcastShape(rowsA, colsA, rowsB, colsB);
		int outLength = outRows * outCols;
		var op = new SpecializedValue<int>((int)operation);

		vectorA.IncrementLiveCount();
		vectorB.IncrementLiveCount();

		int outColumns = StorageColumnsForShape(outRows, outCols);
		Vector output = new(gpu, outLength, outColumns);
		output.IncrementLiveCount();

		LaunchBroadcastOp(
			gpu,
			outLength,
			outCols,
			rowsA,
			colsA,
			rowsB,
			colsB,
			output.GetBuffer().View,
			vectorA.GetBuffer().View,
			vectorB.GetBuffer().View,
			op);

		vectorA.DecrementLiveCount();
		vectorB.DecrementLiveCount();
		output.DecrementLiveCount();
		return output;
	}

	/// <summary>
	/// Runs <c>broadcastOpKernelIP</c>: left operand buffer is both broadcast output and in-place result.
	/// </summary>
	static void RunBroadcastOpIP(
		Vector io,
		Vector other,
		Operations operation,
		int rowsOther,
		int colsOther)
	{
		GPU gpu = io.Gpu;
		(int outRows, int outCols) = io.Shape();
		int outLength = outRows * outCols;
		var op = new SpecializedValue<int>((int)operation);

		io.IncrementLiveCount();
		other.IncrementLiveCount();
		io.IncrementLiveCount();

		LaunchBroadcastOpIP(
			gpu,
			outLength,
			outCols,
			rowsOther,
			colsOther,
			io.GetBuffer().View,
			other.GetBuffer().View,
			op);

		other.DecrementLiveCount();
		io.DecrementLiveCount();
		io.DecrementLiveCount();
	}

	/// <summary>
	/// Launches <c>broadcastOpKernel</c> with <see cref="SpecializedValue{T}"/> shape params for ILGPU specialization.
	/// Operand indexing (row vs column broadcast) is resolved on the GPU from logical rows/cols per operand.
	/// </summary>
	static void LaunchBroadcastOp(
		GPU gpu,
		int outLength,
		int outCols,
		int rowsA,
		int colsA,
		int rowsB,
		int colsB,
		ArrayView<float> output,
		ArrayView<float> inputA,
		ArrayView<float> inputB,
		SpecializedValue<int> operation)
	{
		var stream = gpu.accelerator.DefaultStream;
		gpu.broadcastOpKernel(
			stream,
			outLength,
			output,
			inputA,
			inputB,
			new SpecializedValue<int>(outCols),
			new SpecializedValue<int>(rowsA),
			new SpecializedValue<int>(colsA),
			new SpecializedValue<int>(rowsB),
			new SpecializedValue<int>(colsB),
			operation);
		gpu.accelerator.Synchronize();
	}

	/// <summary>
	/// Launches <c>broadcastOpKernelIP</c> for in-place broadcast; <paramref name="io"/> is the full-size left operand.
	/// </summary>
	static void LaunchBroadcastOpIP(
		GPU gpu,
		int outLength,
		int outCols,
		int rowsOther,
		int colsOther,
		ArrayView<float> io,
		ArrayView<float> other,
		SpecializedValue<int> operation)
	{
		var stream = gpu.accelerator.DefaultStream;
		gpu.broadcastOpKernelIP(
			stream,
			outLength,
			io,
			other,
			new SpecializedValue<int>(outCols),
			new SpecializedValue<int>(rowsOther),
			new SpecializedValue<int>(colsOther),
			operation);
		gpu.accelerator.Synchronize();
	}
}
