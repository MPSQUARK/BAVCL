using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int> appendIntKernel
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(appendIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>> getSliceIntKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(getSliceIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>> reverseIntKernel
		= (_, _, _) => throw new KernelNotCompiledException(nameof(reverseIntKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int> transposeIntKernel
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(transposeIntKernel));

	internal void LoadStructuralInt32Kernels()
	{
		appendIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(AppendIntKernel);
		getSliceIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>>(AccessSliceIntKernel);
		reverseIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>>(ReverseIntKernel);
		transposeIntKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int>(TransposeIntKernel);
	}

	static void AppendIntKernel(Index1D index, ArrayView<int> Output, ArrayView<int> vecA, ArrayView<int> vecB, int vecAcol, int vecBcol)
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

	static void AccessSliceIntKernel(Index1D index, ArrayView<int> OutPut, ArrayView<int> Input, ArrayView<int> ChangeSelectLength)
	{
		OutPut[index] = Input[
			index * ChangeSelectLength[1] +
			ChangeSelectLength[0]];
	}

	static void ReverseIntKernel(Index1D index, ArrayView<int> IO)
	{
		int idx = IO.IntLength - 1 - index;
		(IO[index], IO[idx]) = (IO[idx], IO[index]);
	}

	static void TransposeIntKernel(Index1D index, ArrayView<int> Output, ArrayView<int> Input, int columns)
	{
		int rows = Input.IntLength / columns;
		int col = index % columns;
		int row = (int)XMath.Floor(index / columns);

		int idx = col * rows + row;

		Output[idx] = Input[index];
	}
}
