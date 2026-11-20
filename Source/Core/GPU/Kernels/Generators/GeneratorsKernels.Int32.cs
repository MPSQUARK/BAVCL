using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	internal Action<AcceleratorStream, Index1D, ArrayView<int>, int, long> scaleUniformInt
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(scaleUniformInt), KernelDomain.Generators);

	internal void LoadGeneratorsInt32Kernels() =>
		scaleUniformInt = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, int, long>(ScaleUniformInt_Kern);

	/// <summary>
	/// Maps ILGPU's uniform samples in [0, 2^31), held in <paramref name="output"/>, onto
	/// [<paramref name="minimum"/>, <paramref name="minimum"/> + <paramref name="range"/>) in place.
	/// The 64-bit product scaled down by 2^31 stays in [0, range).
	/// </summary>
	static void ScaleUniformInt_Kern(Index1D index, ArrayView<int> output, int minimum, long range)
	{
		long unit = output[index];
		output[index] = (int)(minimum + ((unit * range) >> 31));
	}
}
