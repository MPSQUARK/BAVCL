using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, SpecializedValue<int>> complexVectorOp =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexVectorOp), KernelDomain.Arithmetic);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, Complex64Device, SpecializedValue<int>> complexScalarOp =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexScalarOp), KernelDomain.Arithmetic);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, SpecializedValue<int>> complexVectorOpIP =
		(_, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexVectorOpIP), KernelDomain.Arithmetic);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, Complex64Device, SpecializedValue<int>> complexScalarOpIP =
		(_, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexScalarOpIP), KernelDomain.Arithmetic);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, SpecializedValue<int>> complexCompareEquals =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexCompareEquals), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<Complex64Device>, Complex64Device, SpecializedValue<int>> complexCompareScalar =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexCompareScalar), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>, Complex64Device> complexMaskFilter =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexMaskFilter), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>> complexMaskPartition =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexMaskPartition), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>, ArrayView<int>> complexGather =
		(_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexGather), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>, ArrayView<int>> complexPartitionGather =
		(_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexPartitionGather), KernelDomain.Mask);

	public Action<AcceleratorStream, Index1D, ArrayView<Complex64Device>, ArrayView<int>, int, int, int, SpecializedValue<int>, SpecializedValue<int>> complexSortPhase =
		(_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(complexSortPhase), KernelDomain.Sorting);

	internal void LoadArithmeticComplex64Kernels()
	{
		complexVectorOp = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, SpecializedValue<int>>(ComplexVectorOp_Kern);
		complexScalarOp = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, Complex64Device, SpecializedValue<int>>(ComplexScalarOp_Kern);
		complexVectorOpIP = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, SpecializedValue<int>>(ComplexVectorOpIP_Kern);
		complexScalarOpIP = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, Complex64Device, SpecializedValue<int>>(ComplexScalarOpIP_Kern);
	}

	internal void LoadMaskComplex64Kernels()
	{
		complexCompareEquals = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<int>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, SpecializedValue<int>>(ComplexCompareEquals_Kern);
		complexCompareScalar = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<int>, ArrayView<Complex64Device>, Complex64Device, SpecializedValue<int>>(ComplexCompareScalar_Kern);
		complexMaskFilter = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>, Complex64Device>(ComplexMaskFilter_Kern);
		complexMaskPartition = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>>(ComplexMaskPartition_Kern);
		complexGather = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>, ArrayView<int>>(ComplexGather_Kern);
		complexPartitionGather = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<Complex64Device>, ArrayView<int>, ArrayView<int>>(ComplexPartitionGather_Kern);
	}

	internal void LoadSortComplex64Kernels()
	{
		complexSortPhase = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<Complex64Device>, ArrayView<int>, int, int, int, SpecializedValue<int>, SpecializedValue<int>>(ComplexSortPhase_Kern);
	}

	static void ComplexVectorOp_Kern(
		Index1D index,
		ArrayView<Complex64Device> output,
		ArrayView<Complex64Device> left,
		ArrayView<Complex64Device> right,
		SpecializedValue<int> operation)
	{
		output[index] = Apply(left[index], right[index], (Operations)operation.Value);
	}

	static void ComplexScalarOp_Kern(
		Index1D index,
		ArrayView<Complex64Device> output,
		ArrayView<Complex64Device> input,
		Complex64Device scalar,
		SpecializedValue<int> operation)
	{
		output[index] = Apply(input[index], scalar, (Operations)operation.Value);
	}

	static void ComplexVectorOpIP_Kern(
		Index1D index,
		ArrayView<Complex64Device> io,
		ArrayView<Complex64Device> right,
		SpecializedValue<int> operation)
	{
		io[index] = Apply(io[index], right[index], (Operations)operation.Value);
	}

	static void ComplexScalarOpIP_Kern(
		Index1D index,
		ArrayView<Complex64Device> io,
		Complex64Device scalar,
		SpecializedValue<int> operation)
	{
		io[index] = Apply(io[index], scalar, (Operations)operation.Value);
	}

	static void ComplexCompareEquals_Kern(
		Index1D index,
		ArrayView<int> output,
		ArrayView<Complex64Device> left,
		ArrayView<Complex64Device> right,
		SpecializedValue<int> comparison)
	{
		Complex64Device a = left[index];
		Complex64Device b = right[index];
		bool equal = a.Real == b.Real && a.Imaginary == b.Imaginary;
		output[index] = equal == (comparison.Value == 0) ? 1 : 0;
	}

	static void ComplexCompareScalar_Kern(
		Index1D index,
		ArrayView<int> output,
		ArrayView<Complex64Device> input,
		Complex64Device scalar,
		SpecializedValue<int> comparison)
	{
		Complex64Device value = input[index];
		bool equal = value.Real == scalar.Real && value.Imaginary == scalar.Imaginary;
		output[index] = equal == (comparison.Value == 0) ? 1 : 0;
	}

	static void ComplexMaskFilter_Kern(
		Index1D index,
		ArrayView<Complex64Device> output,
		ArrayView<Complex64Device> input,
		ArrayView<int> maskWords,
		Complex64Device fill)
	{
		int word = maskWords[index / 32];
		int bit = (word >> (index & 31)) & 1;
		output[index] = bit == 1 ? input[index] : fill;
	}

	static void ComplexMaskPartition_Kern(
		Index1D index,
		ArrayView<Complex64Device> trueOutput,
		ArrayView<Complex64Device> falseOutput,
		ArrayView<Complex64Device> input,
		ArrayView<int> maskWords)
	{
		int word = maskWords[index / 32];
		int bit = (word >> (index & 31)) & 1;
		trueOutput[index] = bit == 1 ? input[index] : new Complex64Device();
		falseOutput[index] = bit == 0 ? input[index] : new Complex64Device();
	}

	static void ComplexGather_Kern(
		Index1D index,
		ArrayView<Complex64Device> output,
		ArrayView<Complex64Device> input,
		ArrayView<int> flags,
		ArrayView<int> writePositions)
	{
		if (flags[index] != 0)
			output[writePositions[index]] = input[index];
	}

	static void ComplexPartitionGather_Kern(
		Index1D index,
		ArrayView<Complex64Device> trueOutput,
		ArrayView<Complex64Device> falseOutput,
		ArrayView<Complex64Device> input,
		ArrayView<int> flags,
		ArrayView<int> writePositions)
	{
		if (flags[index] != 0)
			trueOutput[writePositions[index]] = input[index];
		else
			falseOutput[index - writePositions[index]] = input[index];
	}

	static void ComplexSortPhase_Kern(
		Index1D pair,
		ArrayView<Complex64Device> values,
		ArrayView<int> indices,
		int offset,
		int count,
		int phase,
		SpecializedValue<int> mode,
		SpecializedValue<int> descending)
	{
		int leftIndex = offset + pair * 2 + (phase & 1);
		int rightIndex = leftIndex + 1;
		if (rightIndex >= offset + count)
			return;

		Complex64Device left = values[leftIndex];
		Complex64Device right = values[rightIndex];
		int comparison = Compare(left, right, mode.Value);
		if (descending.Value != 0)
			comparison = -comparison;
		if (comparison <= 0)
			return;

		values[leftIndex] = right;
		values[rightIndex] = left;
		if (indices.Length > 0)
		{
			int index = indices[leftIndex];
			indices[leftIndex] = indices[rightIndex];
			indices[rightIndex] = index;
		}
	}

	static int Compare(Complex64Device left, Complex64Device right, int mode)
	{
		int result;
		if (mode == (int)ComplexSortMode.Complex)
			return CompareComponents(left, right);

		double leftKey = mode == (int)ComplexSortMode.Real ? left.Real :
			mode == (int)ComplexSortMode.Imaginary ? left.Imaginary :
			left.Real * left.Real + left.Imaginary * left.Imaginary;
		double rightKey = mode == (int)ComplexSortMode.Real ? right.Real :
			mode == (int)ComplexSortMode.Imaginary ? right.Imaginary :
			right.Real * right.Real + right.Imaginary * right.Imaginary;
		result = CompareDouble(leftKey, rightKey);
		if (result != 0)
			return result;
		return mode == (int)ComplexSortMode.Imaginary
			? CompareDouble(left.Real, right.Real)
			: CompareDouble(left.Imaginary, right.Imaginary);
	}

	static int CompareComponents(Complex64Device left, Complex64Device right) =>
		CompareDouble(left.Real, right.Real) != 0
			? CompareDouble(left.Real, right.Real)
			: CompareDouble(left.Imaginary, right.Imaginary);

	static int CompareDouble(double left, double right)
	{
		if (double.IsNaN(left))
			return double.IsNaN(right) ? 0 : 1;
		if (double.IsNaN(right))
			return -1;
		return left < right ? -1 : left > right ? 1 : 0;
	}

	static Complex64Device Apply(Complex64Device left, Complex64Device right, Operations operation)
	{
		Complex64Device result = left;
		switch (operation)
		{
			case Operations.add:
				result = new Complex64Device(left.Real + right.Real, left.Imaginary + right.Imaginary);
				break;
			case Operations.subtract:
				result = new Complex64Device(left.Real - right.Real, left.Imaginary - right.Imaginary);
				break;
			case Operations.multiply:
				result = new Complex64Device(
					left.Real * right.Real - left.Imaginary * right.Imaginary,
					left.Real * right.Imaginary + left.Imaginary * right.Real);
				break;
			case Operations.divide:
				result = Divide(left, right);
				break;
		}
		return result;
	}

	static Complex64Device Divide(Complex64Device left, Complex64Device right)
	{
		double scale = Math.Max(Math.Abs(right.Real), Math.Abs(right.Imaginary));
		double real = right.Real / scale;
		double imaginary = right.Imaginary / scale;
		double denominator = real * real + imaginary * imaginary;
		return new(
			(left.Real / scale * real + left.Imaginary / scale * imaginary) / denominator,
			(left.Imaginary / scale * real - left.Real / scale * imaginary) / denominator);
	}
}
