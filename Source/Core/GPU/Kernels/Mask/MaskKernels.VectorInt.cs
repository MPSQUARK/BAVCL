using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> compareMaskInt
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compareMaskInt), KernelDomain.Mask);
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>> compareScalarMaskInt
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compareScalarMaskInt), KernelDomain.Mask);
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, BroadcastStrides, BroadcastStrides> maskFilterInt
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskFilterInt), KernelDomain.Mask);

	internal void LoadMaskVectorIntKernels()
	{
		compareMaskInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(VectorIntCompareMask_Kern);
		compareScalarMaskInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>>(VectorIntScalarCompareMask_Kern);
		maskFilterInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, BroadcastStrides, BroadcastStrides>(VectorIntMaskFilter_Kern);
	}

	static void VectorIntCompareMask_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> left,
		ArrayView<int> right,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		SpecializedValue<int> comparison)
	{
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);

		int lane = CompareLaneInt(
			left[leftStrides.IndexOf(row, column)],
			right[rightStrides.IndexOf(row, column)],
			(VectorComparison)comparison.Value);

		WriteMaskLane(output, element.X, lane);
	}

	static void VectorIntScalarCompareMask_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> input,
		int scalar,
		SpecializedValue<int> comparison) =>
		WriteMaskLane(output, element.X, CompareLaneInt(input[element], scalar, (VectorComparison)comparison.Value));

	static void VectorIntMaskFilter_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> input,
		ArrayView<int> maskWords,
		int fill,
		int outputColumns,
		BroadcastStrides inputStrides,
		BroadcastStrides maskStrides)
	{
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);

		int lane = MaskLane(maskWords, maskStrides.IndexOf(row, column));
		output[element] = Utilities.Select(lane == 1, input[inputStrides.IndexOf(row, column)], fill);
	}
}
