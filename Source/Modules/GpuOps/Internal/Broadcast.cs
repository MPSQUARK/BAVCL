using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.GpuOps;

internal static class Broadcast
{
	internal static Vector BroadcastOP(Vector vectorA, Vector vectorB, Operations operation)
	{
		if (SameAlignedElementwiseLayout(vectorA, vectorB))
			return VectorVectorOp.VectorVectorOP(vectorA, vectorB, operation);

		return RunBroadcastOp(vectorA, vectorB, operation);
	}

	internal static Vector BroadcastIPOP(Vector vector, Vector vectorB, Operations operation)
	{
		if (SameAlignedElementwiseLayout(vector, vectorB))
			return VectorVectorOp.VectorVectorOPIP(vector, vectorB, operation);

		Shape leftShape = vector.Shape();
		Shape outShape = leftShape.BroadcastWith(vectorB.Shape());
		if (leftShape.Rows != outShape.Rows || leftShape.Cols != outShape.Cols)
		{
			throw new PerformanceException(
				"Swap operand order OR use allocating overload 'OP'.");
		}

		RunBroadcastOpIP(vector, vectorB, operation);
		return vector;
	}

	internal static VectorInt BroadcastOP(VectorInt vectorA, VectorInt vectorB, Operations operation)
	{
		if (SameAlignedElementwiseLayout(vectorA, vectorB))
			return VectorVectorOp.VectorVectorOP(vectorA, vectorB, operation);

		return RunBroadcastOp(vectorA, vectorB, operation);
	}

	internal static VectorInt BroadcastIPOP(VectorInt vector, VectorInt vectorB, Operations operation)
	{
		if (SameAlignedElementwiseLayout(vector, vectorB))
			return VectorVectorOp.VectorVectorOPIP(vector, vectorB, operation);

		Shape leftShape = vector.Shape();
		Shape outShape = leftShape.BroadcastWith(vectorB.Shape());
		if (leftShape.Rows != outShape.Rows || leftShape.Cols != outShape.Cols)
		{
			throw new PerformanceException(
				"Swap operand order OR use allocating overload 'OP'.");
		}

		RunBroadcastOpIP(vector, vectorB, operation);
		return vector;
	}

	static bool SameAlignedElementwiseLayout<T>(VectorBase<T> left, VectorBase<T> right)
		where T : unmanaged =>
		left.Shape().MatchesDimensions(right.Shape())
		&& left.Length == right.Length
		&& left.Columns == right.Columns;

	static Vector RunBroadcastOp(Vector vectorA, Vector vectorB, Operations operation)
	{
		Shape shapeA = vectorA.Shape();
		Shape shapeB = vectorB.Shape();
		Shape outShape = shapeA.BroadcastWith(shapeB);

		GPU gpu = vectorA.Gpu;
		int outLength = outShape.ElementCount;
		var op = new SpecializedValue<int>((int)operation);

		Vector output = new(gpu, outLength, outShape.ToStorageColumns());

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

	static void RunBroadcastOpIP(Vector io, Vector other, Operations operation)
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
				BroadcastStrides.For(other.Shape()),
				io.GetBuffer().View,
				other.GetBuffer().View,
				op);
		}
	}

	static VectorInt RunBroadcastOp(VectorInt vectorA, VectorInt vectorB, Operations operation)
	{
		VectorIntOperationValidation.ValidateOperation(operation);

		Shape shapeA = vectorA.Shape();
		Shape shapeB = vectorB.Shape();
		Shape outShape = shapeA.BroadcastWith(shapeB);

		GPU gpu = vectorA.Gpu;
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

	static void RunBroadcastOpIP(VectorInt io, VectorInt other, Operations operation)
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
				BroadcastStrides.For(other.Shape()),
				op);
			gpu.Synchronize();
		}
	}

	static void LaunchBroadcastOp(
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

	static void LaunchBroadcastOpIP(
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
