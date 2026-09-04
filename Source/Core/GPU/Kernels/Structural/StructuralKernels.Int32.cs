using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int> appendInt
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(appendInt));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>> getSliceInt
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(getSliceInt));
	public Action<AcceleratorStream, Index1D, ArrayView<int>> reverseIntIP
		= (_, _, _) => throw new KernelNotCompiledException(nameof(reverseIntIP));
	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int> transposeInt
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(transposeInt));

	internal void LoadStructuralInt32Kernels()
	{
		appendInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int>(AppendInt_Kern);
		getSliceInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>>(AccessSliceInt_Kern);
		reverseIntIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>>(ReverseIntIP_Kern);
		transposeInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int>(TransposeInt_Kern);
	}

	static void AppendInt_Kern(Index1D index, ArrayView<int> Output, ArrayView<int> vecA, ArrayView<int> vecB, int vecAcol, int vecBcol)
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

	static void AccessSliceInt_Kern(Index1D index, ArrayView<int> OutPut, ArrayView<int> Input, ArrayView<int> ChangeSelectLength)
	{
		OutPut[index] = Input[
			index * ChangeSelectLength[1] +
			ChangeSelectLength[0]];
	}

	static void ReverseIntIP_Kern(Index1D index, ArrayView<int> IO)
	{
		int idx = IO.IntLength - 1 - index;
		(IO[index], IO[idx]) = (IO[idx], IO[index]);
	}

	static void TransposeInt_Kern(Index1D index, ArrayView<int> Output, ArrayView<int> Input, int columns)
	{
		int rows = Input.IntLength / columns;
		int col = index % columns;
		int row = (int)XMath.Floor(index / columns);

		int idx = col * rows + row;

		Output[idx] = Input[index];
	}
}
