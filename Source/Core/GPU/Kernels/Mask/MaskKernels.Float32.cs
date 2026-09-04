using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> compareMask
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compareMask));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>, float, SpecializedValue<int>> compareScalarMask
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compareScalarMask));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, float, int, BroadcastStrides, BroadcastStrides> maskFilter
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskFilter));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>> gather
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(gather));

	internal void LoadMaskVectorKernels()
	{
		compareMask = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(VectorCompareMask_Kern);
		compareScalarMask = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>, float, SpecializedValue<int>>(VectorScalarCompareMask_Kern);
		maskFilter = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, float, int, BroadcastStrides, BroadcastStrides>(VectorMaskFilter_Kern);
		gather = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>>(VectorGather_Kern);
	}

	static void VectorCompareMask_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<float> left,
		ArrayView<float> right,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		SpecializedValue<int> comparison)
	{
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);

		int lane = CompareLane(
			left[leftStrides.IndexOf(row, column)],
			right[rightStrides.IndexOf(row, column)],
			(VectorComparison)comparison.Value);

		WriteMaskLane(output, element.X, lane);
	}

	static void VectorScalarCompareMask_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<float> input,
		float scalar,
		SpecializedValue<int> comparison) =>
		WriteMaskLane(output, element.X, CompareLane(input[element], scalar, (VectorComparison)comparison.Value));

	static void VectorMaskFilter_Kern(
		Index1D element,
		ArrayView<float> output,
		ArrayView<float> input,
		ArrayView<int> maskWords,
		float fill,
		int outputColumns,
		BroadcastStrides inputStrides,
		BroadcastStrides maskStrides)
	{
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);

		int lane = MaskLane(maskWords, maskStrides.IndexOf(row, column));
		output[element] = Utilities.Select(lane == 1, input[inputStrides.IndexOf(row, column)], fill);
	}

	static void VectorGather_Kern(
		Index1D element,
		ArrayView<float> output,
		ArrayView<float> input,
		ArrayView<int> sourceIndices) =>
		output[element] = input[sourceIndices[element]];
}
