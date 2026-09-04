using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, SpecializedValue<int>> aOpI
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(aOpI));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>> sOpI
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(sOpI));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, SpecializedValue<int>> aOpIIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(aOpIIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, int, SpecializedValue<int>> sOpIIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(sOpIIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>> reduceRowOpInt
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(reduceRowOpInt));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int> matmulInt
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(matmulInt));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> broadcastInt
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastInt));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, SpecializedValue<int>> broadcastIntIP
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastIntIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>> diffInt
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(diffInt));
	public Action<AcceleratorStream, Index1D, ArrayView<int>> absIntIP
		= (_, _, _) => throw new KernelNotCompiledException(nameof(absIntIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>> negateIntIP
		= (_, _, _) => throw new KernelNotCompiledException(nameof(negateIntIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>> floatToInt
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(floatToInt));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<int>> intToFloat
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(intToFloat));

	internal void LoadArithmeticInt32Kernels()
	{
		aOpI = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, SpecializedValue<int>>(AIntOP_Kern);
		sOpI = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>>(SIntOP_Kern);
		aOpIIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, SpecializedValue<int>>(AIntOPIP_Kern);
		sOpIIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, int, SpecializedValue<int>>(SIntOPIP_Kern);
		reduceRowOpInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, SpecializedValue<int>>(ReduceRowOpInt_Kern);
		matmulInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(MatMulInt_Kern);
		broadcastInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(BroadcastInt_Kern);
		broadcastIntIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, BroadcastStrides, SpecializedValue<int>>(BroadcastIntIP_Kern);
		diffInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>>(DiffInt_Kern);
		absIntIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>>(AbsIntIP_Kern);
		negateIntIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>>(NegateIntIP_Kern);
		floatToInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>>(FloatToInt_Kern);
		intToFloat = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<int>>(IntToFloat_Kern);
		reduceRowFused = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, SpecializedValue<int>>(
			ReduceRowFused_Kern);
	}

	static void AIntOP_Kern(Index1D index, ArrayView<int> OutPut, ArrayView<int> InputA, ArrayView<int> InputB, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref OutPut[index], InputA[index], InputB[index], operation);

	static void AIntOPIP_Kern(Index1D index, ArrayView<int> IO, ArrayView<int> Input, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref IO[index], IO[index], Input[index], operation);

	static void SIntOP_Kern(Index1D index, ArrayView<int> OutPut, ArrayView<int> Input, int Scalar, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref OutPut[index], Input[index], Scalar, operation);

	static void SIntOPIP_Kern(Index1D index, ArrayView<int> IO, int Scalar, SpecializedValue<int> operation) =>
		ApplyBroadcastOpInt(ref IO[index], IO[index], Scalar, operation);

	static void ReduceRowOpInt_Kern(Index1D index, ArrayView<int> output, ArrayView<int> inputA, ArrayView<int> inputB, int cols, SpecializedValue<int> operation)
	{
		int startidx = index * cols;
		output[index] = AccumulateReduceRowInt(inputA, inputB, startidx, cols, operation);
	}

	static void MatMulInt_Kern(
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

	static void BroadcastInt_Kern(
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

	static void BroadcastIntIP_Kern(
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

	static void DiffInt_Kern(Index1D index, ArrayView<int> Output, ArrayView<int> Input) =>
		Output[index] = Input[index + 1] - Input[index];

	static void AbsIntIP_Kern(Index1D index, ArrayView<int> IO) =>
		IO[index] = AbsIntBitwise(IO[index]);

	static void NegateIntIP_Kern(Index1D index, ArrayView<int> IO) =>
		IO[index] = NegateIntBitwise(IO[index]);

	static void FloatToInt_Kern(Index1D index, ArrayView<int> Output, ArrayView<float> Input) =>
		Output[index] = (int)Input[index];

	static void IntToFloat_Kern(Index1D index, ArrayView<float> Output, ArrayView<int> Input) =>
		Output[index] = Input[index];
}
