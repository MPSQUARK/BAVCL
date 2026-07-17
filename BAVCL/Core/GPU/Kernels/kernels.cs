using System;
using System.Diagnostics;
using BAVCL.Core.Exceptions;
using BAVCL.Experimental;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	// TEST KERNELS
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestSQRTKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(TestSQRTKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestMYSQRTKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(TestMYSQRTKernel));

	// CORE KERNELS
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> appendKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(appendKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float> nanToNumKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(nanToNumKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>> getSliceKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(getSliceKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_opFKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(a_opFKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>> s_opFKernel
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(s_opFKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> reduceRowOpKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(reduceRowOpKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> matmulKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(matmulKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>> broadcastOpKernel
		= (_, _, _, _, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastOpKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>> broadcastOpKernelIP
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(broadcastOpKernelIP));

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_FloatOPKernelIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(a_FloatOPKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float, SpecializedValue<int>> s_FloatOPKernelIP
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(s_FloatOPKernelIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> diffKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(diffKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> reverseKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(reverseKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> absKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(absKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> rcpKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(rcpKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> rsqrtKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(rsqrtKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>> crossKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(crossKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int> transposekernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(transposekernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, float> LogKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(LogKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> simdVectorKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(simdVectorKernel));

	public void LoadKernels()
	{
		Stopwatch timer = new();
		timer.Start();

		appendKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);
		nanToNumKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(Nan_to_numKernel);
		getSliceKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernel);

		a_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernel);
		s_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernel);
		reduceRowOpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(ReduceRowOpKernel);
		matmulKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(MatMulKernel);
		broadcastOpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>>(BroadcastOpKernel);
		broadcastOpKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>, SpecializedValue<int>>(BroadcastOpKernelIP);

		simdVectorKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(SIMDVectorKernel);

		a_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernelIP);
		s_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernelIP);

		diffKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>>(DiffKernel);
		reverseKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(ReverseKernel);
		absKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(AbsKernel);
		rcpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(ReciprocalKernel);
		rsqrtKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(RsqrtKernel);

		crossKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(CrossKernel);
		transposekernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int>(TransposeKernel);


		LogKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(LogKern);

		timer.Stop();
		Console.WriteLine($"Kernels Loaded in: {timer.Elapsed.TotalMilliseconds} MS");
	}

	static void AppendKernel(Index1D index, ArrayView<float> Output, ArrayView<float> vecA, ArrayView<float> vecB, int vecAcol, int vecBcol)
	{

		for (int i = 0, j = 0; j < vecBcol; i++)
		{
			if (i < vecAcol)
			{
				Output[index * (vecAcol + vecBcol) + i] = vecA[index * vecAcol + i];
				continue;
			}

			Output[index * (vecAcol + vecBcol) + i] = vecB[index * vecBcol + j];
			j++;
		}
	}

	static void Nan_to_numKernel(Index1D index, ArrayView<float> IO, float num)
	{
		if (float.IsNaN(IO[index]) || float.IsInfinity(IO[index]))
		{
			IO[index] = num;
		}

	}

	static void AccessSliceKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
	{
		OutPut[index] = Input[
			index * ChangeSelectLength[1] +                         // iRcL
			ChangeSelectLength[0]];                                 // Cs
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
	static float AccumulateReduceRow(
		ArrayView<float> coeffs,
		ArrayView<float> inputB,
		int startidx,
		int cols,
		SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += coeffs[i] * inputB[startidx + i];
				return sum;
			}
			case Operations.add:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += coeffs[i] + inputB[startidx + i];
				return sum;
			}
			case Operations.subtract:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += coeffs[i] - inputB[startidx + i];
				return sum;
			}
			case Operations.flipSubtract:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += inputB[startidx + i] - coeffs[i];
				return sum;
			}
			case Operations.divide:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += coeffs[i] / inputB[startidx + i];
				return sum;
			}
			case Operations.flipDivide:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += inputB[startidx + i] / coeffs[i];
				return sum;
			}
			case Operations.pow:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += XMath.Pow(coeffs[i], inputB[startidx + i]);
				return sum;
			}
			case Operations.flipPow:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += XMath.Pow(inputB[startidx + i], coeffs[i]);
				return sum;
			}
			case Operations.differenceSquared:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += XMath.Pow(coeffs[i] - inputB[startidx + i], 2f);
				return sum;
			}
			case Operations.distance:
			{
				float sum = 0f;
				for (int i = 0; i < cols; i++)
					sum += XMath.Pow(coeffs[i] - inputB[startidx + i], 2f);
				return XMath.Sqrt(sum);
			}
			default:
				return 0f;
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

	static void ApplyBroadcastOp(ref float target, float a, float b, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				target = a * b;
				break;
			case Operations.add:
				target = a + b;
				break;
			case Operations.subtract:
				target = a - b;
				break;
			case Operations.flipSubtract:
				target = b - a;
				break;
			case Operations.divide:
				target = a / b;
				break;
			case Operations.flipDivide:
				target = b / a;
				break;
			case Operations.pow:
				target = XMath.Pow(a, b);
				break;
			case Operations.flipPow:
				target = XMath.Pow(b, a);
				break;
			case Operations.differenceSquared:
				target = XMath.Pow(a - b, 2f);
				break;
		}
	}

	static int BroadcastOperandIndex(
		Index1D flatOut,
		SpecializedValue<int> outCols,
		SpecializedValue<int> rows,
		SpecializedValue<int> cols)
	{
		int outColsValue = outCols.Value;
		int rowsValue = rows.Value;
		int colsValue = cols.Value;

		if (rowsValue == 1 && colsValue == 1)
			return 0;

		if (rowsValue == 1)
			return flatOut % outColsValue;

		if (colsValue == 1)
			return flatOut / outColsValue;

		return flatOut;
	}

	static void BroadcastOpKernel(
		Index1D flatOut,
		ArrayView<float> output,
		ArrayView<float> inputA,
		ArrayView<float> inputB,
		SpecializedValue<int> outCols,
		SpecializedValue<int> rowsA,
		SpecializedValue<int> colsA,
		SpecializedValue<int> rowsB,
		SpecializedValue<int> colsB,
		SpecializedValue<int> operation)
	{
		int aIndex = BroadcastOperandIndex(flatOut, outCols, rowsA, colsA);
		int bIndex = BroadcastOperandIndex(flatOut, outCols, rowsB, colsB);
		float result = 0f;
		ApplyBroadcastOp(ref result, inputA[aIndex], inputB[bIndex], operation);
		output[flatOut] = result;
	}

	static void BroadcastOpKernelIP(
		Index1D flatOut,
		ArrayView<float> io,
		ArrayView<float> other,
		SpecializedValue<int> outCols,
		SpecializedValue<int> rowsOther,
		SpecializedValue<int> colsOther,
		SpecializedValue<int> operation)
	{
		int otherIndex = BroadcastOperandIndex(flatOut, outCols, rowsOther, colsOther);
		float result = 0f;
		ApplyBroadcastOp(ref result, io[flatOut], other[otherIndex], operation);
		io[flatOut] = result;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="index"></param>
	/// <param name="Output"></param>
	/// <param name="InputA"></param>
	/// <param name="InputB"></param>
	/// <param name="Cols"></param>
	/// <param name="operation"></param>
	static void SIMDVectorKernel(Index1D index, ArrayView<float> Output, ArrayView<float> InputA, ArrayView<float> InputB, int Cols, SpecializedValue<int> operation)
	{
		int startidx = index * Cols;

		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				for (int i = 0; i < Cols; i++)
					Output[index] += InputA[startidx + i] * InputB[startidx + i];
				break;
			case Operations.add:
				for (int i = 0; i < Cols; i++)
					Output[index] += InputA[startidx + i] + InputB[startidx + i];
				break;
			case Operations.distance:
				for (int i = 0; i < Cols; i++)
					Output[index] += XMath.Pow(InputA[startidx + i] - InputB[startidx + i], 2f);
				Output[index] = XMath.Sqrt(Output[index]);
				break;
			case Operations.magnitude:
				for (int i = 0; i < Cols; i++)
					Output[index] += InputA[startidx + i] * InputB[startidx + i];
				Output[index] = XMath.Sqrt(Output[index]);
				break;
		}
	}

	static void DiffKernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input)
	{
		Output[index] = Input[index + 1] - Input[index];
	}

	static void ReverseKernel(Index1D index, ArrayView<float> IO)
	{
		int idx = IO.IntLength - 1 - index;
		(IO[index], IO[idx]) = (IO[idx], IO[index]);
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

	static void CrossKernel(Index1D index, ArrayView<float> Output, ArrayView<float> InputA, ArrayView<float> InputB)
	{
		Index1D startIdx = index * 3;
		Output[startIdx] = InputA[startIdx + 1] * InputB[startIdx + 2] - InputA[startIdx + 2] * InputB[startIdx + 1];
		Output[startIdx + 1] = InputA[startIdx + 2] * InputB[startIdx] - InputA[startIdx] * InputB[startIdx + 2];
		Output[startIdx + 2] = InputA[startIdx] * InputB[startIdx + 1] - InputA[startIdx + 1] * InputB[startIdx];
	}

	static void TransposeKernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input, int columns)
	{
		int rows = Input.IntLength / columns;
		int col = index % columns;
		int row = (int)XMath.Floor(index / columns);

		int idx = col * rows + row;

		Output[idx] = Input[index];
	}

	public static void LogKern(Index1D index, ArrayView<float> IO, float @base)
	{
		IO[index] = XMath.Log(IO[index], @base);
	}

}