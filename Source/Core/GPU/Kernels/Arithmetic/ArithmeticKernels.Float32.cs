using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float> nanToNumKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(nanToNumKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_opFKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(a_opFKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>> s_opFKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(s_opFKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_FloatOPKernelIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(a_FloatOPKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float, SpecializedValue<int>> s_FloatOPKernelIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(s_FloatOPKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> reduceRowOpKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(reduceRowOpKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> matmulKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(matmulKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> broadcastOpKernel
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastOpKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, SpecializedValue<int>> broadcastOpKernelIP
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastOpKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> diffKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(diffKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> absKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(absKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> rcpKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(rcpKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> rsqrtKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(rsqrtKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float> LogKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(LogKernel));

	internal void LoadArithmeticFloat32Kernels()
	{
		nanToNumKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(Nan_to_numKernel);
		a_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernel);
		s_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernel);
		reduceRowOpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(ReduceRowOpKernel);
		matmulKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(MatMulKernel);
		broadcastOpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(BroadcastOpKernel);
		broadcastOpKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, SpecializedValue<int>>(BroadcastOpKernelIP);
		a_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernelIP);
		s_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernelIP);
		diffKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>>(DiffKernel);
		absKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(AbsKernel);
		rcpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(ReciprocalKernel);
		rsqrtKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(RsqrtKernel);
		LogKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(LogKern);
		reduceRowFusedCompensatedKernel = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int, SpecializedValue<int>>(
			ReduceRowFusedCompensatedKernel);
	}

	static void Nan_to_numKernel(Index1D index, ArrayView<float> IO, float num)
	{
		if (float.IsNaN(IO[index]) || float.IsInfinity(IO[index]))
			IO[index] = num;
	}

	static void A_FloatOPKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				OutPut[index] = InputA[index] * InputB[index];
				break;
			case Operations.add:
				OutPut[index] = InputA[index] + InputB[index];
				break;
			case Operations.subtract:
				OutPut[index] = InputA[index] - InputB[index];
				break;
			case Operations.flipSubtract:
				OutPut[index] = InputB[index] - InputA[index];
				break;
			case Operations.divide:
				OutPut[index] = InputA[index] / InputB[index];
				break;
			case Operations.flipDivide:
				OutPut[index] = InputB[index] / InputA[index];
				break;
			case Operations.pow:
				OutPut[index] = XMath.Pow(InputA[index], InputB[index]);
				break;
			case Operations.flipPow:
				OutPut[index] = XMath.Pow(InputB[index], InputA[index]);
				break;
			case Operations.differenceSquared:
				OutPut[index] = XMath.Pow((InputA[index] - InputB[index]), 2f);
				break;
		}
	}

	static void A_FloatOPKernelIP(Index1D index, ArrayView<float> IO, ArrayView<float> Input, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				IO[index] = IO[index] * Input[index];
				break;
			case Operations.add:
				IO[index] = IO[index] + Input[index];
				break;
			case Operations.subtract:
				IO[index] = IO[index] - Input[index];
				break;
			case Operations.flipSubtract:
				IO[index] = IO[index] - Input[index];
				break;
			case Operations.divide:
				IO[index] = IO[index] / Input[index];
				break;
			case Operations.flipDivide:
				IO[index] = IO[index] / Input[index];
				break;
			case Operations.pow:
				IO[index] = XMath.Pow(IO[index], Input[index]);
				break;
			case Operations.flipPow:
				IO[index] = XMath.Pow(IO[index], Input[index]);
				break;
			case Operations.differenceSquared:
				IO[index] = XMath.Pow((IO[index] - Input[index]), 2f);
				break;
		}
	}

	static void S_FloatOPKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				OutPut[index] = Input[index] * Scalar;
				break;
			case Operations.add:
				OutPut[index] = Input[index] + Scalar;
				break;
			case Operations.subtract:
				OutPut[index] = Input[index] - Scalar;
				break;
			case Operations.flipSubtract:
				OutPut[index] = Scalar - Input[index];
				break;
			case Operations.divide:
				OutPut[index] = Input[index] / Scalar;
				break;
			case Operations.flipDivide:
				OutPut[index] = Scalar / Input[index];
				break;
			case Operations.pow:
				OutPut[index] = XMath.Pow(Input[index], Scalar);
				break;
			case Operations.flipPow:
				OutPut[index] = XMath.Pow(Scalar, Input[index]);
				break;
			case Operations.differenceSquared:
				OutPut[index] = XMath.Pow((Input[index] - Scalar), 2f);
				break;
		}
	}

	static void S_FloatOPKernelIP(Index1D index, ArrayView<float> IO, float Scalar, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				IO[index] = IO[index] * Scalar;
				break;
			case Operations.add:
				IO[index] = IO[index] + Scalar;
				break;
			case Operations.subtract:
				IO[index] = IO[index] - Scalar;
				break;
			case Operations.flipSubtract:
				IO[index] = Scalar - IO[index];
				break;
			case Operations.divide:
				IO[index] = IO[index] / Scalar;
				break;
			case Operations.flipDivide:
				IO[index] = Scalar / IO[index];
				break;
			case Operations.pow:
				IO[index] = XMath.Pow(IO[index], Scalar);
				break;
			case Operations.flipPow:
				IO[index] = XMath.Pow(Scalar, IO[index]);
				break;
			case Operations.differenceSquared:
				IO[index] = XMath.Pow((IO[index] - Scalar), 2f);
				break;
		}
	}

	static void ReduceRowOpKernel(Index1D index, ArrayView<float> output, ArrayView<float> inputA, ArrayView<float> inputB, int cols, SpecializedValue<int> operation)
	{
		int startidx = index * cols;
		output[index] = AccumulateReduceRow(inputA, inputB, startidx, cols, operation);
	}

	static void MatMulKernel(
		Index1D row,
		ArrayView<float> output,
		ArrayView<float> inputA,
		ArrayView<float> inputB,
		int colsA,
		int colsB)
	{
		int outputRow = row * colsB;
		int inputRow = row * colsA;

		for (int col = 0; col < colsB; col++)
		{
			float sum = 0f;
			for (int k = 0; k < colsA; k++)
				sum += inputA[inputRow + k] * inputB[k * colsB + col];

			output[outputRow + col] = sum;
		}
	}

	static void BroadcastOpKernel(
		Index1D flatOut,
		ArrayView<float> output,
		ArrayView<float> inputA,
		ArrayView<float> inputB,
		int outputColumns,
		BroadcastStrides leftStrides,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = flatOut.X / outputColumns;
		int column = flatOut.X - (row * outputColumns);
		int aIndex = leftStrides.IndexOf(row, column);
		int bIndex = rightStrides.IndexOf(row, column);
		float result = 0f;
		ApplyBroadcastOp(ref result, inputA[aIndex], inputB[bIndex], operation);
		output[flatOut] = result;
	}

	static void BroadcastOpKernelIP(
		Index1D flatOut,
		ArrayView<float> io,
		ArrayView<float> other,
		int outputColumns,
		BroadcastStrides rightStrides,
		SpecializedValue<int> operation)
	{
		int row = flatOut.X / outputColumns;
		int column = flatOut.X - (row * outputColumns);
		int otherIndex = rightStrides.IndexOf(row, column);
		float result = 0f;
		ApplyBroadcastOp(ref result, io[flatOut], other[otherIndex], operation);
		io[flatOut] = result;
	}

	static void DiffKernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input)
	{
		Output[index] = Input[index + 1] - Input[index];
	}

	static void AbsKernel(Index1D index, ArrayView<float> IO)
	{
		IO[index] = XMath.Abs(IO[index]);
	}

	static void ReciprocalKernel(Index1D index, ArrayView<float> IO)
	{
		IO[index] = XMath.Rcp(IO[index]);
	}

	static void RsqrtKernel(Index1D index, ArrayView<float> IO)
	{
		IO[index] = XMath.Rsqrt(IO[index]);
	}

	public static void LogKern(Index1D index, ArrayView<float> IO, float @base)
	{
		IO[index] = XMath.Log(IO[index], @base);
	}
}
