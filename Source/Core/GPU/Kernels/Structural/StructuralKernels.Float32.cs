using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> append
		= (_, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(append));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>> getSlice
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(getSlice));
	public Action<AcceleratorStream, Index1D, ArrayView<float>> reverseIP
		= (_, _, _) => throw new KernelNotCompiledException(nameof(reverseIP));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int> transpose
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(transpose));

	internal void LoadStructuralFloat32Kernels()
	{
		append = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(Append_Kern);
		getSlice = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSlice_Kern);
		reverseIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(ReverseIP_Kern);
		transpose = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int>(Transpose_Kern);
	}

	static void Append_Kern(Index1D index, ArrayView<float> Output, ArrayView<float> vecA, ArrayView<float> vecB, int vecAcol, int vecBcol)
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

	static void AccessSlice_Kern(Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
	{
		OutPut[index] = Input[
			index * ChangeSelectLength[1] +
			ChangeSelectLength[0]];
	}

	static void ReverseIP_Kern(Index1D index, ArrayView<float> IO)
	{
		int idx = IO.IntLength - 1 - index;
		(IO[index], IO[idx]) = (IO[idx], IO[index]);
	}

	static void Transpose_Kern(Index1D index, ArrayView<float> Output, ArrayView<float> Input, int columns)
	{
		int rows = Input.IntLength / columns;
		int col = index % columns;
		int row = (int)XMath.Floor(index / columns);

		int idx = col * rows + row;

		Output[idx] = Input[index];
	}
}
