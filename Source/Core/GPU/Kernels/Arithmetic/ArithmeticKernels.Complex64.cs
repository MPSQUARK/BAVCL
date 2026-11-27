using System;
using System.Numerics;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, SpecializedValue<int>> complexVectorOp =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexVectorOp), KernelDomain.Arithmetic);
	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, Complex, SpecializedValue<int>> complexScalarOp =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexScalarOp), KernelDomain.Arithmetic);
	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, SpecializedValue<int>> complexVectorOpIP =
		(_, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexVectorOpIP), KernelDomain.Arithmetic);
	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, Complex, SpecializedValue<int>> complexScalarOpIP =
		(_, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexScalarOpIP), KernelDomain.Arithmetic);
	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> complexBroadcast =
		(_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexBroadcast), KernelDomain.Arithmetic);
	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, int, BroadcastStrides, SpecializedValue<int>> complexBroadcastIP =
		(_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexBroadcastIP), KernelDomain.Arithmetic);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<Complex>, ArrayView<Complex>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> complexCompareMask =
		(_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexCompareMask), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<Complex>, Complex, SpecializedValue<int>> complexCompareScalarMask =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexCompareScalarMask), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, Complex> complexMaskFilter =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexMaskFilter), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, Complex, int, BroadcastStrides, BroadcastStrides> complexMaskFilterBroadcast =
		(_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexMaskFilterBroadcast), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>> complexMaskPartition =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexMaskPartition), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, ArrayView<int>> complexGather =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexGather), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, ArrayView<int>> complexPartitionGather =
		(_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexPartitionGather), KernelDomain.Mask);

	internal void LoadArithmeticComplex64Kernels()
	{
		complexVectorOp = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, SpecializedValue<int>>(ComplexVectorOp_Kern);
		complexScalarOp = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, Complex, SpecializedValue<int>>(ComplexScalarOp_Kern);
		complexVectorOpIP = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, SpecializedValue<int>>(ComplexVectorOpIP_Kern);
		complexScalarOpIP = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, Complex, SpecializedValue<int>>(ComplexScalarOpIP_Kern);
		complexBroadcast = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(ComplexBroadcast_Kern);
		complexBroadcastIP = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, int, BroadcastStrides, SpecializedValue<int>>(ComplexBroadcastIP_Kern);
	}

	internal void LoadMaskComplex64Kernels()
	{
		complexCompareMask = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<int>, ArrayView<Complex>, ArrayView<Complex>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(ComplexCompareMask_Kern);
		complexCompareScalarMask = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<int>, ArrayView<Complex>, Complex, SpecializedValue<int>>(ComplexCompareScalarMask_Kern);
		complexMaskFilter = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, Complex>(ComplexMaskFilter_Kern);
		complexMaskFilterBroadcast = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, Complex, int, BroadcastStrides, BroadcastStrides>(ComplexMaskFilterBroadcast_Kern);
		complexMaskPartition = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>>(ComplexMaskPartition_Kern);
		complexGather = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, ArrayView<int>>(ComplexGather_Kern);
		complexPartitionGather = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex>, ArrayView<Complex>, ArrayView<Complex>, ArrayView<int>, ArrayView<int>>(ComplexPartitionGather_Kern);
	}

	static void ComplexVectorOp_Kern(
		Index1D index,
		ArrayView<Complex> output,
		ArrayView<Complex> left,
		ArrayView<Complex> right,
		SpecializedValue<int> operation)
	{
		Complex result = default;
		ApplyComplexOp(ref result, left[index], right[index], operation);
		output[index] = result;
	}

	static void ComplexVectorOpIP_Kern(
		Index1D index,
		ArrayView<Complex> io,
		ArrayView<Complex> right,
		SpecializedValue<int> operation)
	{
		Complex result = default;
		ApplyComplexOp(ref result, io[index], right[index], operation);
		io[index] = result;
	}

	static void ComplexScalarOp_Kern(
		Index1D index,
		ArrayView<Complex> output,
		ArrayView<Complex> input,
		Complex scalar,
		SpecializedValue<int> operation)
	{
		Complex result = default;
		ApplyComplexOp(ref result, input[index], scalar, operation);
		output[index] = result;
	}

	static void ComplexScalarOpIP_Kern(
		Index1D index,
		ArrayView<Complex> io,
		Complex scalar,
		SpecializedValue<int> operation)
	{
		Complex result = default;
		ApplyComplexOp(ref result, io[index], scalar, operation);
		io[index] = result;
	}

	static void ComplexBroadcast_Kern(
		Index1D index,
		ArrayView<Complex> output,
		ArrayView<Complex> left,
		ArrayView<Complex> right,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = index / outputColumns;
		int column = index - (row * outputColumns);
		Complex result = default;
		ApplyComplexOp(ref result, left[leftStrides.IndexOf(row, column)], right[rightStrides.IndexOf(row, column)], operation);
		output[index] = result;
	}

	static void ComplexBroadcastIP_Kern(
		Index1D index,
		ArrayView<Complex> io,
		ArrayView<Complex> right,
		int outputColumns,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = index / outputColumns;
		int column = index - (row * outputColumns);
		Complex result = default;
		ApplyComplexOp(ref result, io[index], right[rightStrides.IndexOf(row, column)], operation);
		io[index] = result;
	}

	static void ApplyComplexOp(ref Complex target, Complex left, Complex right, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.add:
				target = left + right;
				break;
			case Operations.subtract:
				target = left - right;
				break;
			case Operations.multiply:
				target = left * right;
				break;
			case Operations.divide:
				target = left / right;
				break;
		}
	}

	static void ComplexCompareMask_Kern(
		Index1D index,
		ArrayView<int> output,
		ArrayView<Complex> left,
		ArrayView<Complex> right,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		SpecializedValue<int> comparison)
	{
		int row = index / outputColumns;
		int column = index % outputColumns;
		Complex a = left[leftStrides.IndexOf(row, column)];
		Complex b = right[rightStrides.IndexOf(row, column)];
		bool equal = a == b;
		WriteMaskLane(output, index, equal == (comparison.Value == 0) ? 1 : 0);
	}

	static void ComplexCompareScalarMask_Kern(
		Index1D index,
		ArrayView<int> output,
		ArrayView<Complex> input,
		Complex scalar,
		SpecializedValue<int> comparison)
	{
		Complex value = input[index];
		bool equal = value == scalar;
		WriteMaskLane(output, index, equal == (comparison.Value == 0) ? 1 : 0);
	}

	static void ComplexMaskFilter_Kern(
		Index1D index,
		ArrayView<Complex> output,
		ArrayView<Complex> input,
		ArrayView<int> maskWords,
		Complex fill)
	{
		int word = maskWords[index / 32];
		int bit = (word >> (index & 31)) & 1;
		output[index] = bit == 1 ? input[index] : fill;
	}

	static void ComplexMaskFilterBroadcast_Kern(
		Index1D index,
		ArrayView<Complex> output,
		ArrayView<Complex> input,
		ArrayView<int> maskWords,
		Complex fill,
		int outputColumns,
		BroadcastStrides inputStrides,
		BroadcastStrides maskStrides)
	{
		int row = index / outputColumns;
		int column = index % outputColumns;
		int word = maskWords[maskStrides.IndexOf(row, column) / 32];
		int bit = (word >> (maskStrides.IndexOf(row, column) & 31)) & 1;
		output[index] = bit == 1 ? input[inputStrides.IndexOf(row, column)] : fill;
	}

	static void ComplexMaskPartition_Kern(
		Index1D index,
		ArrayView<Complex> trueOutput,
		ArrayView<Complex> falseOutput,
		ArrayView<Complex> input,
		ArrayView<int> maskWords)
	{
		int word = maskWords[index / 32];
		int bit = (word >> (index & 31)) & 1;
		trueOutput[index] = bit == 1 ? input[index] : new Complex();
		falseOutput[index] = bit == 0 ? input[index] : new Complex();
	}

	static void ComplexGather_Kern(
		Index1D index,
		ArrayView<Complex> output,
		ArrayView<Complex> input,
		ArrayView<int> flags,
		ArrayView<int> writePositions)
	{
		if (flags[index] != 0)
			output[writePositions[index]] = input[index];
	}

	static void ComplexPartitionGather_Kern(
		Index1D index,
		ArrayView<Complex> trueOutput,
		ArrayView<Complex> falseOutput,
		ArrayView<Complex> input,
		ArrayView<int> flags,
		ArrayView<int> writePositions)
	{
		if (flags[index] != 0)
			trueOutput[writePositions[index]] = input[index];
		else
			falseOutput[index - writePositions[index]] = input[index];
	}

}
