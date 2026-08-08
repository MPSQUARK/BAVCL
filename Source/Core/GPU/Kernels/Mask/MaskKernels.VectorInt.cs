using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> vectorIntCompareMaskKernel
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorIntCompareMaskKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>> vectorIntScalarCompareMaskKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorIntScalarCompareMaskKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, BroadcastStrides, BroadcastStrides> vectorIntMaskFilterKernel
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorIntMaskFilterKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>> vectorIntGatherKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(vectorIntGatherKernel));

	internal void LoadMaskVectorIntKernels()
	{
		vectorIntCompareMaskKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(VectorIntCompareMaskKernel);
		vectorIntScalarCompareMaskKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>>(VectorIntScalarCompareMaskKernel);
		vectorIntMaskFilterKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, BroadcastStrides, BroadcastStrides>(VectorIntMaskFilterKernel);
		vectorIntGatherKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>>(VectorIntGatherKernel);
	}

	static void VectorIntCompareMaskKernel(
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

	static void VectorIntScalarCompareMaskKernel(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> input,
		int scalar,
		SpecializedValue<int> comparison) =>
		WriteMaskLane(output, element.X, CompareLaneInt(input[element], scalar, (VectorComparison)comparison.Value));

	static void VectorIntMaskFilterKernel(
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

	static void VectorIntGatherKernel(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> input,
		ArrayView<int> sourceIndices) =>
		output[element] = input[sourceIndices[element]];
}
