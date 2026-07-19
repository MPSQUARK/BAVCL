using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{
	internal static Vector _BroadcastOP(Vector vectorA, Vector vectorB, Operations operation)
	{
		Shape shapeA = vectorA.Shape();
		Shape shapeB = vectorB.Shape();
		EnsureBroadcastable(nameof(OP), shapeA, shapeB);

		if (shapeA.MatchesDimensions(shapeB)
			&& vectorA.Length == vectorB.Length
			&& vectorA.Columns == vectorB.Columns)
			return _VectorVectorOP(vectorA, vectorB, operation);

		return RunBroadcastOp(vectorA, vectorB, operation, shapeA, shapeB);
	}

	internal Vector _BroadcastOP_IP(Vector vectorB, Operations operation)
	{
		Shape leftShape = Shape();
		Shape rightShape = vectorB.Shape();
		EnsureBroadcastable(nameof(IPOP), leftShape, rightShape);

		Shape outShape = leftShape.BroadcastWith(rightShape);
		if (leftShape.Rows != outShape.Rows || leftShape.Cols != outShape.Cols)
		{
			throw new PerformanceException(
				"Swap operand order OR use allocating overload 'OP'.");
		}

		if (leftShape.MatchesDimensions(rightShape)
			&& Length == vectorB.Length
			&& Columns == vectorB.Columns)
			return _VectorVectorOP_IP(vectorB, operation);

		RunBroadcastOpIP(this, vectorB, operation, rightShape);
		return this;
	}

	static void EnsureBroadcastable(string operation, Shape shapeA, Shape shapeB)
	{
		try
		{
			shapeA.BroadcastWith(shapeB);
		}
		catch (ShapeMismatchException)
		{
			throw new ShapeMismatchException(operation, shapeA.ToString(), shapeB.ToString());
		}
	}

	static Vector RunBroadcastOp(
		Vector vectorA,
		Vector vectorB,
		Operations operation,
		Shape shapeA,
		Shape shapeB)
	{
		GPU gpu = vectorA.Gpu;
		Shape outShape = shapeA.BroadcastWith(shapeB);
		int outLength = outShape.ElementCount;
		var op = new SpecializedValue<int>((int)operation);

		int outColumns = outShape.ToStorageColumns();
		Vector output = new(gpu, outLength, outColumns);

		using (GpuScope.Begin(output, vectorA, vectorB))
		{
			LaunchBroadcastOp(
				gpu,
				outLength,
				outShape.Cols,
				shapeA.Rows,
				shapeA.Cols,
				shapeB.Rows,
				shapeB.Cols,
				output.GetBuffer().View,
				vectorA.GetBuffer().View,
				vectorB.GetBuffer().View,
				op);
		}

		return output;
	}

	static void RunBroadcastOpIP(
		Vector io,
		Vector other,
		Operations operation,
		Shape shapeOther)
	{
		GPU gpu = io.Gpu;
		Shape outShape = io.Shape();
		int outLength = outShape.ElementCount;
		var op = new SpecializedValue<int>((int)operation);

		using (GpuScope.Begin(io, other))
		{
			LaunchBroadcastOpIP(
				gpu,
				outLength,
				outShape.Cols,
				shapeOther.Rows,
				shapeOther.Cols,
				io.GetBuffer().View,
				other.GetBuffer().View,
				op);
		}
	}

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
