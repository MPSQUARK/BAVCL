using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> vectorCompareMaskKernel
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorCompareMaskKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>, float, SpecializedValue<int>> vectorScalarCompareMaskKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorScalarCompareMaskKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, float, int, BroadcastStrides, BroadcastStrides> vectorMaskFilterKernel
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorMaskFilterKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>> vectorGatherKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorGatherKernel));

	internal void LoadMaskVectorKernels()
	{
		vectorCompareMaskKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(VectorCompareMaskKernel);
		vectorScalarCompareMaskKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>, float, SpecializedValue<int>>(VectorScalarCompareMaskKernel);
		vectorMaskFilterKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, float, int, BroadcastStrides, BroadcastStrides>(VectorMaskFilterKernel);
		vectorGatherKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>>(VectorGatherKernel);
	}

	static void VectorCompareMaskKernel(
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

	static void VectorScalarCompareMaskKernel(
		Index1D element,
		ArrayView<int> output,
		ArrayView<float> input,
		float scalar,
		SpecializedValue<int> comparison) =>
		WriteMaskLane(output, element.X, CompareLane(input[element], scalar, (VectorComparison)comparison.Value));

	static void VectorMaskFilterKernel(
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

	static void VectorGatherKernel(
		Index1D element,
		ArrayView<float> output,
		ArrayView<float> input,
		ArrayView<int> sourceIndices) =>
		output[element] = input[sourceIndices[element]];
}
