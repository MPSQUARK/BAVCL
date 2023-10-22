using System;
using System.Diagnostics;
using BAVCL.Experimental;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL
{
	public partial class GPU
	{
		// TEST KERNELS
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestSQRTKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestMYSQRTKernel;
		
		// CORE KERNELS
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> appendKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, float> nanToNumKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>> getSliceKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_opFKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>> s_opFKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> vectormatrixOpKernel;

		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_FloatOPKernelIP;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, float, SpecializedValue<int>> s_FloatOPKernelIP;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> diffKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> reverseKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> absKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> rcpKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> rsqrtKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>> crossKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int> transposekernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, float> LogKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> simdVectorKernel
		
		
		public void LoadKernels()
		{
			Stopwatch timer = new();
			timer.Start();

			appendKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);
			nanToNumKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(Nan_to_numKernel);
			getSliceKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernel);
			
			a_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernel);
			s_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernel);
			vectormatrixOpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(VectorMatrixKernel);

			simdVectorKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(SIMDVectorKernel);

			a_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernelIP);
			s_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernelIP);

			diffKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>> (DiffKernel);
			reverseKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>> (ReverseKernel);
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
		static void VectorMatrixKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, int Cols, SpecializedValue<int> operation)
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
						OutPut[index] += XMath.Pow(InputA[i], InputB[startidx + i]);
					break;
				case Operations.flipPow:
					for (int i = 0; i < Cols; i++)
						OutPut[index] += XMath.Pow(InputB[startidx + i], InputA[i]);
					break;
				case Operations.differenceSquared:
					for (int i = 0; i < Cols; i++)
						OutPut[index] += XMath.Pow(InputA[i] - InputB[startidx + i], 2f);
					break;
				case Operations.distance:
					for (int i = 0; i < Cols; i++)
						OutPut[index] += XMath.Pow(InputA[i] - InputB[startidx + i], 2f);
					OutPut[index] = XMath.Sqrt(OutPut[index]);
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
			Output[startIdx]     = InputA[startIdx + 1] * InputB[startIdx + 2] - InputA[startIdx + 2] * InputB[startIdx + 1];
			Output[startIdx + 1] = InputA[startIdx + 2] * InputB[startIdx    ] - InputA[startIdx    ] * InputB[startIdx + 2];
			Output[startIdx + 2] = InputA[startIdx    ] * InputB[startIdx + 1] - InputA[startIdx + 1] * InputB[startIdx    ];
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
			IO[index] = XMath.Log(IO[index],@base);
		}
		
	}
}