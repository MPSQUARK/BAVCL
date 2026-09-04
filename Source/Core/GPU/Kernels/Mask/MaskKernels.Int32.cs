using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, SpecializedValue<int>> maskWordOp
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskWordOp));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, int, int, SpecializedValue<int>> maskWordConstOp
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskWordConstOp));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> maskLaneOp
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskLaneOp));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, SpecializedValue<int>> maskLaneOpIP
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskLaneOpIP));

	internal void LoadMaskWordKernels()
	{
		maskWordOp = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, SpecializedValue<int>>(MaskWordOp_Kern);
		maskWordConstOp = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, int, int, SpecializedValue<int>>(MaskWordConstOp_Kern);
		maskLaneOp = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(MaskLaneOp_Kern);
		maskLaneOpIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, SpecializedValue<int>>(MaskLaneOpIP_Kern);
	}

	// One thread per packed word: a single bitwise instruction resolves 32 mask lanes.
	// Passing the same view as output and left performs the operation in place.
	static void MaskWordOp_Kern(
		Index1D word,
		ArrayView<int> output,
		ArrayView<int> left,
		ArrayView<int> right,
		int lastWord,
		int tailMask,
		SpecializedValue<int> operation) =>
		output[word] = ApplyMaskWordOp(left[word], right[word], (MaskOperation)operation.Value)
			& PaddingKeepMask(word, lastWord, tailMask);

	// Complement, set-all and clear-all are word operations against a constant operand.
	static void MaskWordConstOp_Kern(
		Index1D word,
		ArrayView<int> output,
		ArrayView<int> input,
		int operand,
		int lastWord,
		int tailMask,
		SpecializedValue<int> operation) =>
		output[word] = ApplyMaskWordOp(input[word], operand, (MaskOperation)operation.Value)
			& PaddingKeepMask(word, lastWord, tailMask);

	// Broadcasting breaks word alignment, so operands are addressed per lane instead.
	static void MaskLaneOp_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> left,
		ArrayView<int> right,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);

		int lane = ApplyMaskWordOp(
			MaskLane(left, leftStrides.IndexOf(row, column)),
			MaskLane(right, rightStrides.IndexOf(row, column)),
			(MaskOperation)operation.Value) & 1;

		WriteMaskLane(output, element.X, lane);
	}

	// In place the destination already carries the output layout, so only the right operand
	// broadcasts. Each thread owns one lane, so clear-then-set cannot race a neighbouring lane.
	static void MaskLaneOpIP_Kern(
		Index1D element,
		ArrayView<int> io,
		ArrayView<int> right,
		int outputColumns,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);

		int lane = ApplyMaskWordOp(
			MaskLane(io, element.X),
			MaskLane(right, rightStrides.IndexOf(row, column)),
			(MaskOperation)operation.Value) & 1;

		int word = element.X >> MaskWordShift;
		int shift = element.X & MaskLaneMask;

		Atomic.And(ref io[word], ~(1 << shift));
		Atomic.Or(ref io[word], lane << shift);
	}
}
