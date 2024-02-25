using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.CustomMath;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	private Dictionary<(Type, KernelType), Delegate> _kernels = new();
	
	public Signature GetKernel<Signature, T>(KernelType kernel) where Signature : Delegate where T : unmanaged
		=> (Signature)_kernels[(typeof(T), kernel)];
	public Delegate GetKernel<T>(KernelType kernel) where T : unmanaged
		=> _kernels[(typeof(T), kernel)];
	
	public void RegisterKernels<T>() where T : unmanaged
	{
		_kernels.Add(
			(typeof(T), KernelType.Append),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>, ArrayView<T>, int, int>(AppendKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Access),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>, ArrayView<int>>(AccessSliceKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Reverse),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>>(ReverseKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Transpose),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>, int>(TransposeKernel)
		);
	}
	
	public void RegisterNumberKernels<T>() where T : unmanaged, INumber<T>
	{
		_kernels.Add((typeof(T), KernelType.NanToNum),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, T>(Nan_to_numKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.SeqOP),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>, ArrayView<T>, SpecializedValue<int>>(SeqOPKern)
		);
		_kernels.Add(
			(typeof(T), KernelType.SeqIPOP),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>, SpecializedValue<int>>(SeqIPOPKern)
		);
		_kernels.Add(
			(typeof(T), KernelType.ScalarOP),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>, T, SpecializedValue<int>>(ScalarOPKern)
		);
		_kernels.Add(
			(typeof(T), KernelType.ScalarIPOP),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, T, SpecializedValue<int>>(ScalarIPOPKern)
		);
		_kernels.Add(
			(typeof(T), KernelType.Diff),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>>(DiffKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Abs),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>>(AbsKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Reciprocal),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>>(ReciprocalKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Rsqrt),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>>(RsqrtKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Cross),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, ArrayView<T>, ArrayView<T>>(CrossKernel)
		);
		_kernels.Add(
			(typeof(T), KernelType.Log),
			accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<T>, T>(LogKern)
		);
		
	}
	
	public void RegisterDefaultKernels()
	{
		Stopwatch sw = Stopwatch.StartNew();
		RegisterKernels<float>();
		RegisterNumberKernels<float>();
		sw.Stop();
		Console.WriteLine($"Default Kernels Registered in: {sw.Elapsed.TotalMilliseconds} MS");
	}	
	
	static void AppendKernel<T>(Index1D index, ArrayView<T> Output, ArrayView<T> vecA, ArrayView<T> vecB, int vecAcol, int vecBcol)
		where T : unmanaged
	{

		for (int i = 0, j=0; j < vecBcol; i++)
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

	static void Nan_to_numKernel<T>(Index1D index, ArrayView<T> IO, T num) where T : unmanaged, INumber<T>
	{
		if (T.IsNaN(IO[index]) || T.IsInfinity(IO[index]))
			IO[index] = num;
	}

	static void AccessSliceKernel<T>(Index1D index, ArrayView<T> OutPut, ArrayView<T> Input, ArrayView<int> ChangeSelectLength)
		where T : unmanaged
	{
						// iRcL                             + //Cs
		OutPut[index] = Input[index * ChangeSelectLength[1] + ChangeSelectLength[0]];
	}
	
	static void SeqOPKern<T>(Index1D index, ArrayView<T> OutPut, ArrayView<T> InputA, ArrayView<T> InputB, SpecializedValue<int> operation)
		where T : unmanaged, INumber<T>
	{
		switch ((Operations)operation.Value)
		{
			case Operations.add:
				OutPut[index] = InputA[index] + InputB[index];
				break;
			case Operations.subtract:
				OutPut[index] = InputA[index] - InputB[index];
				break;
			case Operations.flipSubtract:
				OutPut[index] = InputB[index] - InputA[index];
				break;
			case Operations.multiply:
				OutPut[index] = InputA[index] * InputB[index];
				break;
			case Operations.divide:
				OutPut[index] = InputA[index] / InputB[index];
				break;
			case Operations.flipDivide:
				OutPut[index] = InputB[index] / InputA[index];
				break;
			case Operations.pow:
				OutPut[index] = CMath.Pow(InputA[index], InputB[index]);
				break;
			case Operations.flipPow:
				OutPut[index] = CMath.Pow(InputB[index], InputA[index]);
				break;
			case Operations.differenceSquared:
				OutPut[index] = CMath.Square(InputA[index] - InputB[index]);
				break;
		}
	}

	static void SeqIPOPKern<T>(Index1D index, ArrayView<T> IO, ArrayView<T> Input, SpecializedValue<int> operation)
		where T : unmanaged, INumber<T>
	{
		switch ((Operations)operation.Value)
		{
			case Operations.add:
				IO[index] = IO[index] + Input[index];
				break;
			case Operations.subtract:
				IO[index] = IO[index] - Input[index];
				break;
			case Operations.flipSubtract:
				IO[index] = Input[index] - IO[index];
				break;
			case Operations.multiply:
				IO[index] = IO[index] * Input[index];
				break;
			case Operations.divide:
				IO[index] = IO[index] / Input[index];
				break;
			case Operations.flipDivide:
				IO[index] = Input[index] / IO[index];
				break;
			case Operations.pow:
				IO[index] = CMath.Pow(IO[index], Input[index]);
				break;
			case Operations.flipPow:
				IO[index] = CMath.Pow(Input[index], IO[index]);
				break;
			case Operations.differenceSquared:
				IO[index] = CMath.Square(IO[index] - Input[index]);
				break;
		}
	}

	static void ScalarOPKern<T>(Index1D index, ArrayView<T> OutPut, ArrayView<T> Input, T Scalar, SpecializedValue<int> operation)
		where T : unmanaged, INumber<T>
	{
		switch ((Operations)operation.Value)
		{
			case Operations.add:
				OutPut[index] = Input[index] + Scalar;
				break;
			case Operations.subtract:
				OutPut[index] = Input[index] - Scalar;
				break;
			case Operations.flipSubtract:
				OutPut[index] = Scalar - Input[index];
				break;
			case Operations.multiply:
				OutPut[index] = Input[index] * Scalar;
				break;
			case Operations.divide:
				OutPut[index] = Input[index] / Scalar;
				break;
			case Operations.flipDivide:
				OutPut[index] = Scalar / Input[index];
				break;
			case Operations.pow:
				OutPut[index] = CMath.Pow(Input[index], Scalar);
				break;
			case Operations.flipPow:
				OutPut[index] = CMath.Pow(Scalar, Input[index]);
				break;
			case Operations.differenceSquared:
				OutPut[index] = CMath.Square(Input[index] - Scalar);
				break;	
		}
	}

	static void ScalarIPOPKern<T>(Index1D index, ArrayView<T> IO, T Scalar, SpecializedValue<int> operation)
		where T : unmanaged, INumber<T>
	{
		switch ((Operations)operation.Value)
		{
			case Operations.add:
				IO[index] = IO[index] + Scalar;
				break;
			case Operations.subtract:
				IO[index] = IO[index] - Scalar;
				break;
			case Operations.flipSubtract:
				IO[index] = Scalar - IO[index];
				break;
			case Operations.multiply:
				IO[index] = IO[index] * Scalar;
				break;
			case Operations.divide:
				IO[index] = IO[index] / Scalar;
				break;
			case Operations.flipDivide:
				IO[index] = Scalar / IO[index];
				break;
			case Operations.pow:
				IO[index] = CMath.Pow(IO[index], Scalar);
				break;
			case Operations.flipPow:
				IO[index] = CMath.Pow(Scalar, IO[index]);
				break;
			case Operations.differenceSquared:
				IO[index] = CMath.Square(IO[index] - Scalar);
				break;
		}
	}


	static void VectorMatrixKernel<T>(Index1D index, ArrayView<T> OutPut, ArrayView<T> InputA, ArrayView<T> InputB, int Cols, SpecializedValue<int> operation)
		where T : unmanaged, INumber<T>
	{
		int startidx = index * Cols;

		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += InputA[i] * InputB[startidx + i];
				break;
			case Operations.add:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += InputA[i] + InputB[startidx + i];
				break;
			case Operations.subtract:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += InputA[i] - InputB[startidx + i];
				break;
			case Operations.flipSubtract:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += InputB[startidx + i] - InputA[i];
				break;
			case Operations.divide:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += InputA[i] / InputB[startidx + i];
				break;
			case Operations.flipDivide:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += InputB[startidx + i] / InputA[i];
				break;
			case Operations.pow:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += CMath.Pow(InputA[i], InputB[startidx + i]);
				break;
			case Operations.flipPow:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += CMath.Pow(InputB[startidx + i], InputA[i]);
				break;
			case Operations.differenceSquared:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += CMath.Square(InputA[i] - InputB[startidx + i]);
				break;
			case Operations.distance:
				for (int i = 0; i < Cols; i++)
					OutPut[index] += CMath.Square(InputA[i] - InputB[startidx + i]);
				OutPut[index] = CMath.Sqrt(OutPut[index]);
				break;
		}
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
	static void SIMDVectorKernel<T>(Index1D index, ArrayView<T> Output, ArrayView<T> InputA, ArrayView<T> InputB, int Cols, SpecializedValue<int> operation)
		where T : unmanaged, INumber<T>
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
					Output[index] += CMath.Square(InputA[startidx + i] - InputB[startidx + i]);
				Output[index] = CMath.Sqrt(Output[index]);
				break;
			case Operations.magnitude:
				for (int i = 0; i < Cols; i++)
					Output[index] += InputA[startidx + i] * InputB[startidx + i];
				Output[index] = CMath.Sqrt(Output[index]);
				break;				
		}
	}

	// TODO: Maybe this can be merged into the OP kernel?
	static void DiffKernel<T>(Index1D index, ArrayView<T> Output, ArrayView<T> Input)
		where T : unmanaged, INumber<T>
	{
		Output[index] = Input[index + 1] - Input[index];
	}

	// TODO: This and other kernels only using one input could be merged?
	static void ReverseKernel<T>(Index1D index, ArrayView<T> IO) where T: unmanaged
	{
		int idx = IO.IntLength - 1 - index;
		(IO[index], IO[idx]) = (IO[idx], IO[index]);
	}

	static void AbsKernel<T>(Index1D index, ArrayView<T> IO) where T: unmanaged, INumber<T>
	{
		IO[index] = CMath.Abs(IO[index]);
	}

	static void ReciprocalKernel<T>(Index1D index, ArrayView<T> IO) where T: unmanaged, INumber<T>
	{
		IO[index] = CMath.Rcp(IO[index]);
	}

	static void RsqrtKernel<T>(Index1D index, ArrayView<T> IO) where T: unmanaged, INumber<T>
	{
		IO[index] = CMath.Rsqrt(IO[index]);
	}

	static void CrossKernel<T>(Index1D index, ArrayView<T> Output, ArrayView<T> InputA, ArrayView<T> InputB)
		where T : unmanaged, INumber<T>
	{
		Index1D startIdx = index * 3;
		Output[startIdx]     = InputA[startIdx + 1] * InputB[startIdx + 2] - InputA[startIdx + 2] * InputB[startIdx + 1];
		Output[startIdx + 1] = InputA[startIdx + 2] * InputB[startIdx    ] - InputA[startIdx    ] * InputB[startIdx + 2];
		Output[startIdx + 2] = InputA[startIdx    ] * InputB[startIdx + 1] - InputA[startIdx + 1] * InputB[startIdx    ];
	}

	static void TransposeKernel<T>(Index1D index, ArrayView<T> Output, ArrayView<T> Input, int columns)
		where T : unmanaged
	{
		int rows = Input.IntLength / columns;
		int col = index % columns;
		int row = (int)XMath.Floor(index / columns);

		int idx = col * rows + row;

		Output[idx] = Input[index];
	}

	public static void LogKern<T>(Index1D index, ArrayView<T> IO, T @base)
		where T : unmanaged, INumber<T>
	{
		IO[index] = CMath.Log(IO[index], @base);
	}
	
}
