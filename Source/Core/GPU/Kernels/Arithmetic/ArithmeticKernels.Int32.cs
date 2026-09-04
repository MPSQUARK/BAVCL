using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, SpecializedValue<int>> a_opIKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(a_opIKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>> s_opIKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(s_opIKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, SpecializedValue<int>> a_opIKernelIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(a_opIKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, int, SpecializedValue<int>> s_opIKernelIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(s_opIKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>> reduceRowOpIntKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(reduceRowOpIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int> matmulIntKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(matmulIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> broadcastOpIntKernel
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastOpIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, SpecializedValue<int>> broadcastOpIntKernelIP
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastOpIntKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>> diffIntKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(diffIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>> absIntKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(absIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>> negateIntKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(negateIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>> floatToIntKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(floatToIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<int>> intToFloatKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(intToFloatKernel));

	internal void LoadArithmeticInt32Kernels()
	{
		a_opIKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, SpecializedValue<int>>(A_IntOPKernel);
		s_opIKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>>(S_IntOPKernel);
		a_opIKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, SpecializedValue<int>>(A_IntOPKernelIP);
		s_opIKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, int, SpecializedValue<int>>(S_IntOPKernelIP);
		reduceRowOpIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>>(ReduceRowOpIntKernel);
		matmulIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(MatMulIntKernel);
		broadcastOpIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(BroadcastOpIntKernel);
		broadcastOpIntKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, SpecializedValue<int>>(BroadcastOpIntKernelIP);
		diffIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>>(DiffIntKernel);
		absIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>>(AbsIntKernel);
		negateIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>>(NegateIntKernel);
		floatToIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>>(FloatToIntKernel);
		intToFloatKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<int>>(IntToFloatKernel);
		reduceRowFusedKernel = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, SpecializedValue<int>>(
			ReduceRowFusedKernel);
	}

	static void A_IntOPKernel(Index1D index, ArrayView<int> OutPut, ArrayView<int> InputA, ArrayView<int> InputB, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref OutPut[index], InputA[index], InputB[index], operation);

	static void A_IntOPKernelIP(Index1D index, ArrayView<int> IO, ArrayView<int> Input, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref IO[index], IO[index], Input[index], operation);

	static void S_IntOPKernel(Index1D index, ArrayView<int> OutPut, ArrayView<int> Input, int Scalar, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref OutPut[index], Input[index], Scalar, operation);

	static void S_IntOPKernelIP(Index1D index, ArrayView<int> IO, int Scalar, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref IO[index], IO[index], Scalar, operation);

	static void ReduceRowOpIntKernel(Index1D index, ArrayView<int> output, ArrayView<int> inputA, ArrayView<int> inputB, int cols, SpecializedValue<int> operation)
	{
		int startidx = index * cols;
		output[index] = AccumulateReduceRowInt(inputA, inputB, startidx, cols, operation);
	}

	static void MatMulIntKernel(
		Index1D row,
		ArrayView<int> output,
		ArrayView<int> inputA,
		ArrayView<int> inputB,
		int colsA,
		int colsB)
	{
		int outputRow = row * colsB;
		int inputRow = row * colsA;

		for (int col = 0; col < colsB; col++)
		{
			int sum = 0;
			for (int k = 0; k < colsA; k++)
				sum += inputA[inputRow + k] * inputB[k * colsB + col];

			output[outputRow + col] = sum;
		}
	}

	static void BroadcastOpIntKernel(
		Index1D flatOut,
		ArrayView<int> output,
		ArrayView<int> inputA,
		ArrayView<int> inputB,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = flatOut.X / outputColumns;
		int column = flatOut.X - (row * outputColumns);
		int aIndex = leftStrides.IndexOf(row, column);
		int bIndex = rightStrides.IndexOf(row, column);
		int result = 0;
		ApplyBroadcastOpInt(ref result, inputA[aIndex], inputB[bIndex], operation);
		output[flatOut] = result;
	}

	static void BroadcastOpIntKernelIP(
		Index1D flatOut,
		ArrayView<int> io,
		ArrayView<int> other,
		int outputColumns,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = flatOut.X / outputColumns;
		int column = flatOut.X - (row * outputColumns);
		int otherIndex = rightStrides.IndexOf(row, column);
		int result = 0;
		ApplyBroadcastOpInt(ref result, io[flatOut], other[otherIndex], operation);
		io[flatOut] = result;
	}

	static void DiffIntKernel(Index1D index, ArrayView<int> Output, ArrayView<int> Input) =>
		Output[index] = Input[index + 1] - Input[index];

	static void AbsIntKernel(Index1D index, ArrayView<int> IO) =>
		IO[index] = AbsIntBitwise(IO[index]);

	static void NegateIntKernel(Index1D index, ArrayView<int> IO) =>
		IO[index] = NegateIntBitwise(IO[index]);

	static void FloatToIntKernel(Index1D index, ArrayView<int> Output, ArrayView<float> Input) =>
		Output[index] = (int)Input[index];

	static void IntToFloatKernel(Index1D index, ArrayView<float> Output, ArrayView<int> Input) =>
		Output[index] = Input[index];
}
