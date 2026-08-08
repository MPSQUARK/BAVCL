using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Util;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<int>> floatToSortableIntKern
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(floatToSortableIntKern));

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<float>> sortableIntToFloatKern
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(sortableIntToFloatKern));

	internal void LoadSortFloatKernels()
	{
		floatToSortableIntKern = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<int>>(FloatToSortableIntKern);
		sortableIntToFloatKern = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<float>>(SortableIntToFloatKern);
		LoadSortSegmentedFloatRadixKernels();
	}

	static void FloatToSortableIntKern(Index1D index, ArrayView<float> input, ArrayView<int> output)
	{
		int bits = (int)Interop.FloatAsInt(input[index]);
		output[index] = FloatBitsToSortable(bits);
	}

	static void SortableIntToFloatKern(Index1D index, ArrayView<int> input, ArrayView<float> output)
	{
		int bits = SortableToFloatBits(input[index]);
		output[index] = Interop.IntAsFloat((uint)bits);
	}
}
