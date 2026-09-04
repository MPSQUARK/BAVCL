using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float> nanToNumIP
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(nanToNumIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> aOpF
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(aOpF));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>> sOpF
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(sOpF));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> aOpFIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(aOpFIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float, SpecializedValue<int>> sOpFIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(sOpFIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> reduceRowOp
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(reduceRowOp));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> matmul
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(matmul));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>> broadcast
		= (_, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcast));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, SpecializedValue<int>> broadcastIP
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> diff
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(diff));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> absIP
		= (_, _, _) => throw new KernelNotCompiledException(nameof(absIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> rcpIP
		= (_, _, _) => throw new KernelNotCompiledException(nameof(rcpIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> rsqrtIP
		= (_, _, _) => throw new KernelNotCompiledException(nameof(rsqrtIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float> logIP
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(logIP));

	internal void LoadArithmeticFloat32Kernels()
	{
		nanToNumIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(NanToNumIP_Kern);
		aOpF = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(AFloatOP_Kern);
		sOpF = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>>(SFloatOP_Kern);
		reduceRowOp = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(ReduceRowOp_Kern);
		matmul = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(MatMul_Kern);
		broadcast = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, BroadcastStrides, SpecializedValue<int>>(Broadcast_Kern);
		broadcastIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int, BroadcastStrides, SpecializedValue<int>>(BroadcastIP_Kern);
		aOpFIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(AFloatOPIP_Kern);
		sOpFIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, SpecializedValue<int>>(SFloatOPIP_Kern);
		diff = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>>(Diff_Kern);
		absIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(AbsIP_Kern);
		rcpIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(ReciprocalIP_Kern);
		rsqrtIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(RsqrtIP_Kern);
		logIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(LogIP_Kern);
		reduceRowFusedCompensated = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int, SpecializedValue<int>>(
			ReduceRowFusedCompensated_Kern);
	}

	static void NanToNumIP_Kern(Index1D index, ArrayView<float> IO, float num)
	{
		if (float.IsNaN(IO[index]) || float.IsInfinity(IO[index]))
			IO[index] = num;
	}

	static void AFloatOP_Kern(Index1D index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, SpecializedValue<int> operation)
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

	static void AFloatOPIP_Kern(Index1D index, ArrayView<float> IO, ArrayView<float> Input, SpecializedValue<int> operation)
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

	static void SFloatOP_Kern(Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar, SpecializedValue<int> operation)
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

	static void SFloatOPIP_Kern(Index1D index, ArrayView<float> IO, float Scalar, SpecializedValue<int> operation)
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

	static void ReduceRowOp_Kern(Index1D index, ArrayView<float> output, ArrayView<float> inputA, ArrayView<float> inputB, int cols, SpecializedValue<int> operation)
	{
		int startidx = index * cols;
		output[index] = AccumulateReduceRow(inputA, inputB, startidx, cols, operation);
	}

	static void MatMul_Kern(
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

	static void Broadcast_Kern(
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

	static void BroadcastIP_Kern(
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

	static void Diff_Kern(Index1D index, ArrayView<float> Output, ArrayView<float> Input)
	{
		Output[index] = Input[index + 1] - Input[index];
	}

	static void AbsIP_Kern(Index1D index, ArrayView<float> IO)
	{
		IO[index] = XMath.Abs(IO[index]);
	}

	static void ReciprocalIP_Kern(Index1D index, ArrayView<float> IO)
	{
		IO[index] = XMath.Rcp(IO[index]);
	}

	static void RsqrtIP_Kern(Index1D index, ArrayView<float> IO)
	{
		IO[index] = XMath.Rsqrt(IO[index]);
	}

	public static void LogIP_Kern(Index1D index, ArrayView<float> IO, float @base)
	{
		IO[index] = XMath.Log(IO[index], @base);
	}
}
