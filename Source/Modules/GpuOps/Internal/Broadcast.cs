using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.GpuOps;

internal static class Broadcast
{
	internal static Vector BroadcastOP(Vector vectorA, Vector vectorB, Operations operation)
	{
		Shape shapeA = vectorA.Shape();
		Shape shapeB = vectorB.Shape();
		EnsureBroadcastable($"{nameof(GpuOpsModule)}.OP", shapeA, shapeB);

		if (shapeA.MatchesDimensions(shapeB)
			&& vectorA.Length == vectorB.Length
			&& vectorA.Columns == vectorB.Columns)
			return VectorVectorOp.VectorVectorOP(vectorA, vectorB, operation);

		return RunBroadcastOp(vectorA, vectorB, operation, shapeA, shapeB);
	}

	internal static Vector BroadcastIPOP(Vector vector, Vector vectorB, Operations operation)
	{
		Shape leftShape = vector.Shape();
		Shape rightShape = vectorB.Shape();
		EnsureBroadcastable($"{nameof(GpuOpsModule)}.IPOP", leftShape, rightShape);

		Shape outShape = leftShape.BroadcastWith(rightShape);
		if (leftShape.Rows != outShape.Rows || leftShape.Cols != outShape.Cols)
		{
			throw new PerformanceException(
				"Swap operand order OR use allocating overload 'OP'.");
		}

		if (leftShape.MatchesDimensions(rightShape)
			&& vector.Length == vectorB.Length
			&& vector.Columns == vectorB.Columns)
			return VectorVectorOp.VectorVectorOPIP(vector, vectorB, operation);

		RunBroadcastOpIP(vector, vectorB, operation, rightShape);
		return vector;
	}

	internal static void EnsureBroadcastable(string operation, Shape shapeA, Shape shapeB)
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

	internal static Vector RunBroadcastOp(
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
				BroadcastStrides.For(shapeA),
				BroadcastStrides.For(shapeB),
				output.GetBuffer().View,
				vectorA.GetBuffer().View,
				vectorB.GetBuffer().View,
				op);
		}

		return output;
	}

	internal static void RunBroadcastOpIP(
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
				BroadcastStrides.For(shapeOther),
				io.GetBuffer().View,
				other.GetBuffer().View,
				op);
		}
	}

	internal static void LaunchBroadcastOp(
		GPU gpu,
		int outLength,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		ArrayView<float> output,
		ArrayView<float> inputA,
		ArrayView<float> inputB,
		SpecializedValue<int> operation)
	{
		var stream = gpu.accelerator.DefaultStream;
		gpu.broadcast(
			stream,
			outLength,
			output,
			inputA,
			inputB,
			outputColumns,
			leftStrides,
			rightStrides,
			operation);
		gpu.accelerator.Synchronize();
	}

	internal static void LaunchBroadcastOpIP(
		GPU gpu,
		int outLength,
		int outputColumns,
		BroadcastStrides rightStrides,
		ArrayView<float> io,
		ArrayView<float> other,
		SpecializedValue<int> operation)
	{
		var stream = gpu.accelerator.DefaultStream;
		gpu.broadcastIP(
			stream,
			outLength,
			io,
			other,
			outputColumns,
			rightStrides,
			operation);
		gpu.accelerator.Synchronize();
	}

	internal static VectorInt BroadcastOP(VectorInt vectorA, VectorInt vectorB, Operations operation)
	{
		Shape shapeA = vectorA.Shape();
		Shape shapeB = vectorB.Shape();
		EnsureBroadcastable($"{nameof(GpuOpsModule)}.OP", shapeA, shapeB);

		if (shapeA.MatchesDimensions(shapeB)
			&& vectorA.Length == vectorB.Length
			&& vectorA.Columns == vectorB.Columns)
			return VectorVectorOp.VectorVectorOP(vectorA, vectorB, operation);

		return RunBroadcastOp(vectorA, vectorB, operation, shapeA, shapeB);
	}

	internal static VectorInt BroadcastIPOP(VectorInt vector, VectorInt vectorB, Operations operation)
	{
		Shape leftShape = vector.Shape();
		Shape rightShape = vectorB.Shape();
		EnsureBroadcastable($"{nameof(GpuOpsModule)}.IPOP", leftShape, rightShape);

		Shape outShape = leftShape.BroadcastWith(rightShape);
		if (leftShape.Rows != outShape.Rows || leftShape.Cols != outShape.Cols)
		{
			throw new PerformanceException(
				"Swap operand order OR use allocating overload 'OP'.");
		}

		if (leftShape.MatchesDimensions(rightShape)
			&& vector.Length == vectorB.Length
			&& vector.Columns == vectorB.Columns)
			return VectorVectorOp.VectorVectorOPIP(vector, vectorB, operation);

		RunBroadcastOpIP(vector, vectorB, operation, rightShape);
		return vector;
	}

	internal static VectorInt RunBroadcastOp(
		VectorInt vectorA,
		VectorInt vectorB,
		Operations operation,
		Shape shapeA,
		Shape shapeB)
	{
		VectorIntOperationValidation.ValidateOperation(operation);
		GPU gpu = vectorA.Gpu;
		Shape outShape = shapeA.BroadcastWith(shapeB);
		RequireBroadcastKernelOperands(vectorA, vectorB, outShape);
		int outLength = outShape.ElementCount;
		var op = new SpecializedValue<int>((int)operation);

		VectorInt output = new(gpu, outLength, outShape.ToStorageColumns());

		using (GpuScope.Begin(output, vectorA, vectorB))
		{
			gpu.broadcastInt(
				gpu.DefaultStream,
				outLength,
				output.GetBuffer().View,
				vectorA.GetBuffer().View,
				vectorB.GetBuffer().View,
				outShape.Cols,
				BroadcastStrides.For(shapeA),
				BroadcastStrides.For(shapeB),
				op);
			gpu.Synchronize();
		}

		return output;
	}

	internal static void RunBroadcastOpIP(
		VectorInt io,
		VectorInt other,
		Operations operation,
		Shape shapeOther)
	{
		VectorIntOperationValidation.ValidateOperation(operation);
		GPU gpu = io.Gpu;
		Shape outShape = io.Shape();
		RequireBroadcastKernelOperands(io, other, outShape);
		int outLength = outShape.ElementCount;
		var op = new SpecializedValue<int>((int)operation);

		using (GpuScope.Begin(io, other))
		{
			gpu.broadcastIntIP(
				gpu.DefaultStream,
				outLength,
				io.GetBuffer().View,
				other.GetBuffer().View,
				outShape.Cols,
				BroadcastStrides.For(shapeOther),
				op);
			gpu.Synchronize();
		}
	}

	static void RequireBroadcastKernelOperands(VectorInt vectorA, VectorInt vectorB, Shape outShape)
	{
		if (vectorA.Columns == 0 || vectorB.Columns == 0)
		{
			throw new ArgumentException(
				"Gpu broadcast requires 2D storage layout (Columns > 0) on all operands.");
		}

		if (outShape.Cols <= 0)
		{
			throw new ArgumentException(
				"Gpu broadcast requires a matrix-shaped output (Columns > 0).");
		}
	}
}
