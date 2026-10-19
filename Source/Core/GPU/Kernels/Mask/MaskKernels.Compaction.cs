using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	internal Reduction<int, Stride1D.Dense> ReduceAddInt32
		= (_, _, _) => throw new KernelNotCompiledException(nameof(ReduceAddInt32), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides> maskBroadcastFlags
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskBroadcastFlags), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides> compactGatherFloat
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compactGatherFloat), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides> compactGatherFloatPartition
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compactGatherFloatPartition), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides> compactGatherInt
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compactGatherInt), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides> compactGatherIntPartition
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compactGatherIntPartition), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides> compactWriteIndices
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(compactWriteIndices), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, int> maskPopcountPartial
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskPopcountPartial), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, int> maskAnyPartial
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskAnyPartial), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, int> maskAllPartial
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskAllPartial), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, int> maskFirstTrueIndex
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskFirstTrueIndex), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, int> maskLastTrueIndex
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(maskLastTrueIndex), KernelDomain.Mask);

	internal void LoadMaskCompactionKernels()
	{
		ReduceAddInt32 = accelerator.CreateReduction<int, Stride1D.Dense, AddInt32>();
		maskBroadcastFlags = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides>(MaskBroadcastFlags_Kern);
		compactGatherFloat = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides>(CompactGatherFloat_Kern);
		compactGatherFloatPartition = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides>(CompactGatherFloatPartition_Kern);
		compactGatherInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides>(CompactGatherInt_Kern);
		compactGatherIntPartition = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides>(CompactGatherIntPartition_Kern);
		compactWriteIndices = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides>(CompactWriteIndices_Kern);
		maskPopcountPartial = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, int>(MaskPopcountPartial_Kern);
		maskAnyPartial = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, int>(MaskAnyPartial_Kern);
		maskAllPartial = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, int>(MaskAllPartial_Kern);
		maskFirstTrueIndex = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, int>(MaskFirstTrueIndex_Kern);
		maskLastTrueIndex = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, int>(MaskLastTrueIndex_Kern);
	}

	static void MaskBroadcastFlags_Kern(
		Index1D element,
		ArrayView<int> flags,
		ArrayView<int> maskWords,
		int outputColumns,
		BroadcastStrides maskStrides)
	{
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);
		flags[element] = MaskLane(maskWords, maskStrides.IndexOf(row, column));
	}

	static void CompactGatherFloatPartition_Kern(
		Index1D element,
		ArrayView<float> trueOutput,
		ArrayView<float> falseOutput,
		ArrayView<float> input,
		ArrayView<int> flags,
		ArrayView<int> writePos,
		int outputColumns,
		BroadcastStrides inputStrides)
	{
		int flag = flags[element];
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);
		float value = input[inputStrides.IndexOf(row, column)];
		int write = writePos[element];
		if (flag != 0)
			trueOutput[write] = value;
		else
			falseOutput[element.X - write] = value;
	}

	static void CompactGatherFloat_Kern(
		Index1D element,
		ArrayView<float> output,
		ArrayView<float> input,
		ArrayView<int> flags,
		ArrayView<int> writePos,
		int outputColumns,
		BroadcastStrides inputStrides)
	{
		if (flags[element] == 0)
			return;

		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);
		output[writePos[element]] = input[inputStrides.IndexOf(row, column)];
	}

	static void CompactGatherIntPartition_Kern(
		Index1D element,
		ArrayView<int> trueOutput,
		ArrayView<int> falseOutput,
		ArrayView<int> input,
		ArrayView<int> flags,
		ArrayView<int> writePos,
		int outputColumns,
		BroadcastStrides inputStrides)
	{
		int flag = flags[element];
		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);
		int value = input[inputStrides.IndexOf(row, column)];
		int write = writePos[element];
		if (flag != 0)
			trueOutput[write] = value;
		else
			falseOutput[element.X - write] = value;
	}

	static void CompactGatherInt_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> input,
		ArrayView<int> flags,
		ArrayView<int> writePos,
		int outputColumns,
		BroadcastStrides inputStrides)
	{
		if (flags[element] == 0)
			return;

		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);
		output[writePos[element]] = input[inputStrides.IndexOf(row, column)];
	}

	static void CompactWriteIndices_Kern(
		Index1D element,
		ArrayView<int> output,
		ArrayView<int> flags,
		ArrayView<int> writePos,
		int outputColumns,
		BroadcastStrides sourceStrides)
	{
		if (flags[element] == 0)
			return;

		int row = element.X / outputColumns;
		int column = element.X - (row * outputColumns);
		output[writePos[element]] = sourceStrides.IndexOf(row, column);
	}

	static void MaskPopcountPartial_Kern(
		Index1D word,
		ArrayView<int> partial,
		ArrayView<int> maskWords,
		int lastWord,
		int tailMask)
	{
		int value = maskWords[word] & PaddingKeepMask(word, lastWord, tailMask);
		partial[word] = PopCount32(value);
	}

	static void MaskAnyPartial_Kern(
		Index1D word,
		ArrayView<int> partial,
		ArrayView<int> maskWords,
		int lastWord,
		int tailMask)
	{
		int value = maskWords[word] & PaddingKeepMask(word, lastWord, tailMask);
		partial[word] = BoolToInt(value != 0);
	}

	static void MaskAllPartial_Kern(
		Index1D word,
		ArrayView<int> partial,
		ArrayView<int> maskWords,
		int lastWord,
		int tailMask)
	{
		int keep = PaddingKeepMask(word, lastWord, tailMask);
		int value = maskWords[word] & keep;
		partial[word] = BoolToInt(value == keep);
	}

	static void MaskFirstTrueIndex_Kern(
		Index1D word,
		ArrayView<int> maskWords,
		ArrayView<int> partial,
		int lastWord,
		int tailMask)
	{
		int value = maskWords[word] & PaddingKeepMask(word, lastWord, tailMask);
		if (value == 0)
		{
			partial[word] = int.MaxValue;
			return;
		}

		int bit = TrailingZeroCount32(value);
		partial[word] = (word << MaskWordShift) + bit;
	}

	static void MaskLastTrueIndex_Kern(
		Index1D word,
		ArrayView<int> maskWords,
		ArrayView<int> partial,
		int lastWord,
		int tailMask)
	{
		int value = maskWords[word] & PaddingKeepMask(word, lastWord, tailMask);
		if (value == 0)
		{
			partial[word] = -1;
			return;
		}

		int bit = 31 - LeadingZeroCount32(value);
		partial[word] = (word << MaskWordShift) + bit;
	}

	/// <summary>SWAR popcount for 32-bit lanes (device-side; no hardware POPCNT dependency).</summary>
	static int PopCount32(int value)
	{
		value -= (value >> 1) & 0x55555555;
		value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
		return (((value + (value >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
	}

	static int TrailingZeroCount32(int value)
	{
		if (value == 0)
			return 32;

		int count = 0;
		for (int bit = 0; bit < 32; bit++)
		{
			if ((value & (1 << bit)) != 0)
				return count;

			count++;
		}

		return 32;
	}

	static int LeadingZeroCount32(int value)
	{
		if (value == 0)
			return 32;

		for (int shift = 31; shift >= 0; shift--)
		{
			if ((value & (1 << shift)) != 0)
				return 31 - shift;
		}

		return 32;
	}
}
