using BAVCL.Core.Exceptions;
using BAVCL.Core.Helpers;
using BAVCL.Modules.GpuOps;
using BAVCL.Types;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

internal static class MaskBitwiseOps
{
	const int AllLanes = -1;
	const int NoLanes = 0;

	internal static Mask BinaryOp(Mask left, Mask right, MaskOperation operation)
	{
		Shape outputShape = OutputShape(left, right, operation);
		Mask output = new(left.Gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		if (SharesLayout(left, right))
			LaunchWordOp(output, left, right, operation);
		else
			LaunchLaneOp(output, left, right, outputShape, operation);

		return output;
	}

	internal static Mask BinaryOpInPlace(Mask left, Mask right, MaskOperation operation)
	{
		Shape outputShape = OutputShape(left, right, operation);

		if (!left.Shape().MatchesDimensions(outputShape))
			throw new PerformanceException("Swap operand order OR use the allocating mask operator.");

		if (SharesLayout(left, right))
			LaunchWordOpInPlace(left, right, operation);
		else
			LaunchLaneOpInPlace(left, right, outputShape, operation);

		return left;
	}

	internal static Mask Complement(Mask mask)
	{
		Mask output = EmptyLike(mask);
		LaunchConstOp(output, mask, AllLanes, MaskOperation.Xor);
		return output;
	}

	internal static Mask AllSet(Mask layout)
	{
		Mask output = EmptyLike(layout);
		LaunchConstOpInPlace(output, AllLanes, MaskOperation.Or);
		return output;
	}

	/// <summary>Fresh mask storage is already zeroed, so an all-false mask needs no kernel.</summary>
	internal static Mask AllClear(Mask layout) => EmptyLike(layout);

	internal static void SetAll(Mask mask) => LaunchConstOpInPlace(mask, AllLanes, MaskOperation.Or);

	internal static void ClearAll(Mask mask) => LaunchConstOpInPlace(mask, NoLanes, MaskOperation.And);

	static void LaunchWordOp(Mask output, Mask left, Mask right, MaskOperation operation)
	{
		using (GpuScope.Begin(output, left, right))
			DispatchWordOp(output, left, right, operation);
	}

	static void LaunchWordOpInPlace(Mask io, Mask right, MaskOperation operation)
	{
		using (GpuScope.Begin(io, right))
			DispatchWordOp(io, io, right, operation);
	}

	static void DispatchWordOp(Mask output, Mask left, Mask right, MaskOperation operation)
	{
		GPU gpu = output.Gpu;

		gpu.maskWordOp(
			gpu.DefaultStream,
			output.WordCount,
			output.GetBuffer().View,
			left.GetBuffer().View,
			right.GetBuffer().View,
			output.WordCount - 1,
			MaskBitOps.TailMask(output.ElementCount),
			new SpecializedValue<int>((int)operation));

		gpu.Synchronize();
	}

	static void LaunchConstOp(Mask output, Mask input, int operand, MaskOperation operation)
	{
		using (GpuScope.Begin(output, input))
			DispatchConstOp(output, input, operand, operation);
	}

	static void LaunchConstOpInPlace(Mask io, int operand, MaskOperation operation)
	{
		using (GpuScope.Begin(io))
			DispatchConstOp(io, io, operand, operation);
	}

	static void DispatchConstOp(Mask output, Mask input, int operand, MaskOperation operation)
	{
		GPU gpu = output.Gpu;

		gpu.maskWordConstOp(
			gpu.DefaultStream,
			output.WordCount,
			output.GetBuffer().View,
			input.GetBuffer().View,
			operand,
			output.WordCount - 1,
			MaskBitOps.TailMask(output.ElementCount),
			new SpecializedValue<int>((int)operation));

		gpu.Synchronize();
	}

	static void LaunchLaneOp(Mask output, Mask left, Mask right, Shape outputShape, MaskOperation operation)
	{
		GPU gpu = output.Gpu;

		using (GpuScope.Begin(output, left, right))
		{
			gpu.maskLaneOp(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				left.GetBuffer().View,
				right.GetBuffer().View,
				outputShape.Cols,
				BroadcastStrides.For(left.Shape()),
				BroadcastStrides.For(right.Shape()),
				new SpecializedValue<int>((int)operation));

			gpu.Synchronize();
		}
	}

	static void LaunchLaneOpInPlace(Mask io, Mask right, Shape outputShape, MaskOperation operation)
	{
		GPU gpu = io.Gpu;

		using (GpuScope.Begin(io, right))
		{
			gpu.maskLaneOpIP(
				gpu.DefaultStream,
				outputShape.ElementCount,
				io.GetBuffer().View,
				right.GetBuffer().View,
				outputShape.Cols,
				BroadcastStrides.For(right.Shape()),
				new SpecializedValue<int>((int)operation));

			gpu.Synchronize();
		}
	}

	static Shape OutputShape(Mask left, Mask right, MaskOperation operation)
	{
		Shape leftShape = left.Shape();
		Shape rightShape = right.Shape();
		Broadcast.EnsureBroadcastable($"{nameof(Mask)}.{operation}", leftShape, rightShape);
		return leftShape.BroadcastWith(rightShape);
	}

	static bool SharesLayout(Mask left, Mask right) => left.Shape().MatchesDimensions(right.Shape());

	static Mask EmptyLike(Mask mask) => new(mask.Gpu, mask.ElementCount, mask.Columns);
}
