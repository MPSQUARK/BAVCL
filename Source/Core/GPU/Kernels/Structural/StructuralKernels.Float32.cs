using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> appendKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(appendKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>> getSliceKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(getSliceKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> reverseKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(reverseKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int> transposekernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(transposekernel));

	internal void LoadStructuralFloat32Kernels()
	{
		appendKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);
		getSliceKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernel);
		reverseKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(ReverseKernel);
		transposekernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int>(TransposeKernel);
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

	static void AccessSliceKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
	{
		OutPut[index] = Input[
			index * ChangeSelectLength[1] +
			ChangeSelectLength[0]];
	}

	static void ReverseKernel(Index1D index, ArrayView<float> IO)
	{
		int idx = IO.IntLength - 1 - index;
		(IO[index], IO[idx]) = (IO[idx], IO[index]);
	}

	static void TransposeKernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input, int columns)
	{
		int rows = Input.IntLength / columns;
		int col = index % columns;
		int row = (int)XMath.Floor(index / columns);

		int idx = col * rows + row;

		Output[idx] = Input[index];
	}
}
