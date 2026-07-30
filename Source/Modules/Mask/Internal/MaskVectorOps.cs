using System;
using BAVCL.Core.Helpers;
using BAVCL.Modules.GpuOps;
using BAVCL.Types;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

internal static class MaskVectorOps
{
	internal static Mask Compare(Vector left, Vector right, VectorComparison comparison)
	{
		Shape leftShape = left.Shape();
		Shape rightShape = right.Shape();
		Shape outputShape = OutputShape(leftShape, rightShape, nameof(Compare));

		GPU gpu = left.Gpu;
		Mask output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, left, right))
		{
			gpu.vectorCompareMaskKernel(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				left.GetBuffer().View,
				right.GetBuffer().View,
				outputShape.Cols,
				BroadcastStrides.For(leftShape),
				BroadcastStrides.For(rightShape),
				new SpecializedValue<int>((int)comparison));

			gpu.Synchronize();
		}

		return output;
	}

	internal static Mask Compare(Vector vector, float scalar, VectorComparison comparison)
	{
		GPU gpu = vector.Gpu;
		Mask output = new(gpu, vector.Length, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			gpu.vectorScalarCompareMaskKernel(
				gpu.DefaultStream,
				vector.Length,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				scalar,
				new SpecializedValue<int>((int)comparison));

			gpu.Synchronize();
		}

		return output;
	}

	internal static Vector Filter(Vector vector, Mask mask, float fill)
	{
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = OutputShape(vectorShape, maskShape, nameof(Filter));

		GPU gpu = vector.Gpu;
		Vector output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, vector, mask))
		{
			gpu.vectorMaskFilterKernel(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				mask.GetBuffer().View,
				fill,
				outputShape.Cols,
				BroadcastStrides.For(vectorShape),
				BroadcastStrides.For(maskShape));

			gpu.Synchronize();
		}

		return output;
	}

	internal static Vector Select(Vector vector, Mask mask)
	{
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = OutputShape(vectorShape, maskShape, nameof(Select));

		int[] sourceIndices = SelectedSourceIndices(mask, maskShape, vectorShape, outputShape);
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, sourceIndices.Length, 0);

		if (sourceIndices.Length == 0)
			return output;

		// Scratch indices live outside the LRU cache: they are freed before this call returns
		// and registering them would only churn the eviction order.
		using (GpuScope.Begin(output, vector))
		using (MemoryBuffer1D<int, Stride1D.Dense> indices = gpu.accelerator.Allocate1D(sourceIndices))
		{
			gpu.vectorGatherKernel(
				gpu.DefaultStream,
				sourceIndices.Length,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				indices.View);

			gpu.Synchronize();
		}

		return output;
	}

	// Compaction cannot size its output until the surviving lanes are known, and a Vector must be
	// allocated with that length, so the mask is resolved host-side and only the gather runs on device.
	static int[] SelectedSourceIndices(Mask mask, Shape maskShape, Shape vectorShape, Shape outputShape)
	{
		BroadcastStrides maskStrides = BroadcastStrides.For(maskShape);
		BroadcastStrides vectorStrides = BroadcastStrides.For(vectorShape);
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();

		int[] selected = new int[outputShape.ElementCount];
		int count = 0;

		for (int row = 0; row < outputShape.Rows; row++)
		{
			for (int column = 0; column < outputShape.Cols; column++)
			{
				if (!MaskBitOps.GetBit(words, maskStrides.IndexOf(row, column)))
					continue;

				selected[count++] = vectorStrides.IndexOf(row, column);
			}
		}

		return selected[..count];
	}

	static Shape OutputShape(Shape left, Shape right, string operation)
	{
		Broadcast.EnsureBroadcastable($"{nameof(Vector)}.{operation}", left, right);
		return left.BroadcastWith(right);
	}
}
