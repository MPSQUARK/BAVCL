using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>> crossKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(crossKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> normaliseKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(normaliseKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> simdVectorKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(simdVectorKernel));

	internal void LoadGeometryFloat32Kernels()
	{
		crossKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(CrossKernel);
		normaliseKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>>(NormaliseKernel);
		simdVectorKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(SIMDVectorKernel);
	}

	static void CrossKernel(Index1D index, ArrayView<float> Output, ArrayView<float> InputA, ArrayView<float> InputB)
	{
		Index1D startIdx = index + index + index;
		Output[startIdx] = InputA[startIdx + 1] * InputB[startIdx + 2] - InputA[startIdx + 2] * InputB[startIdx + 1];
		Output[startIdx + 1] = InputA[startIdx + 2] * InputB[startIdx] - InputA[startIdx] * InputB[startIdx + 2];
		Output[startIdx + 2] = InputA[startIdx] * InputB[startIdx + 1] - InputA[startIdx + 1] * InputB[startIdx];
	}

	static void NormaliseKernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input)
	{
		Index1D startIdx = index + index + index;
		float x = Input[startIdx];
		float y = Input[startIdx + 1];
		float z = Input[startIdx + 2];
		float invMag = XMath.Rsqrt(x * x + y * y + z * z);
		Output[startIdx] = x * invMag;
		Output[startIdx + 1] = y * invMag;
		Output[startIdx + 2] = z * invMag;
	}

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
}
