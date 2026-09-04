using System;
using BAVCL.Core.Helpers;
using BAVCL.Modules.GpuOps;
using BAVCL.Types;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

internal static class MaskVectorIntOps
{
	internal static Mask CompareX(VectorInt left, VectorInt right, VectorComparison comparison)
	{
		Shape leftShape = left.Shape();
		Shape rightShape = right.Shape();
		Shape outputShape = OutputShape(leftShape, rightShape, nameof(CompareX));

		GPU gpu = left.Gpu;
		Mask output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, left, right))
		{
			gpu.compareMaskInt(
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

	internal static Mask CompareX(VectorInt vector, int scalar, VectorComparison comparison)
	{
		GPU gpu = vector.Gpu;
		Mask output = new(gpu, vector.Length, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			gpu.compareScalarMaskInt(
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

	internal static VectorInt MaskX(VectorInt vector, Mask mask, int fill)
	{
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = OutputShape(vectorShape, maskShape, nameof(MaskX));

		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, vector, mask))
		{
			gpu.maskFilterInt(
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

	internal static VectorInt FilterX(VectorInt vector, Mask mask)
	{
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = OutputShape(vectorShape, maskShape, nameof(FilterX));

		int[] sourceIndices = SelectedSourceIndices(mask, maskShape, vectorShape, outputShape, selected: true);
		return Gather(vector, sourceIndices);
	}

	internal static (VectorInt TrueLanes, VectorInt FalseLanes) PartitionX(VectorInt vector, Mask mask)
	{
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = OutputShape(vectorShape, maskShape, nameof(PartitionX));

		int[] trueIndices = SelectedSourceIndices(mask, maskShape, vectorShape, outputShape, selected: true);
		int[] falseIndices = SelectedSourceIndices(mask, maskShape, vectorShape, outputShape, selected: false);

		return (Gather(vector, trueIndices), Gather(vector, falseIndices));
	}

	static VectorInt Gather(VectorInt vector, int[] sourceIndices)
	{
		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, sourceIndices.Length, 0);

		if (sourceIndices.Length == 0)
			return output;

		using (GpuScope.Begin(output, vector))
		using (MemoryBuffer1D<int, Stride1D.Dense> indices = gpu.accelerator.Allocate1D(sourceIndices))
		{
			gpu.gatherInt(
				gpu.DefaultStream,
				sourceIndices.Length,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				indices.View);

			gpu.Synchronize();
		}

		return output;
	}

	static int[] SelectedSourceIndices(Mask mask, Shape maskShape, Shape vectorShape, Shape outputShape, bool selected)
	{
		BroadcastStrides maskStrides = BroadcastStrides.For(maskShape);
		BroadcastStrides vectorStrides = BroadcastStrides.For(vectorShape);
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();

		int[] indices = new int[outputShape.ElementCount];
		int count = 0;

		for (int row = 0; row < outputShape.Rows; row++)
		{
			for (int column = 0; column < outputShape.Cols; column++)
			{
				bool bit = MaskBitOps.GetBit(words, maskStrides.IndexOf(row, column));
				if (bit != selected)
					continue;

				indices[count++] = vectorStrides.IndexOf(row, column);
			}
		}

		return indices[..count];
	}

	static Shape OutputShape(Shape left, Shape right, string operation)
	{
		Broadcast.EnsureBroadcastable($"{nameof(VectorInt)}.{operation}", left, right);
		return left.BroadcastWith(right);
	}
}
