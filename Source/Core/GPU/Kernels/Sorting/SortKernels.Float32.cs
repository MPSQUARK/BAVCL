using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<int>> floatToSortableInt
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(floatToSortableInt));

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>> sortableIntToFloat
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(sortableIntToFloat));

	internal void LoadSortFloatKernels()
	{
		floatToSortableInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<int>>(FloatToSortableInt_Kern);
		sortableIntToFloat = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>>(SortableIntToFloat_Kern);
		LoadSortSegmentedFloatRadixKernels();
	}

	static void FloatToSortableInt_Kern(Index1D index, ArrayView<float> input, ArrayView<int> output)
	{
		int bits = (int)Interop.FloatAsInt(input[index]);
		output[index] = FloatBitsToSortable(bits);
	}

	static void SortableIntToFloat_Kern(Index1D index, ArrayView<int> input, ArrayView<float> output)
	{
		int bits = SortableToFloatBits(input[index]);
		output[index] = Interop.IntAsFloat((uint)bits);
	}
}
