using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms.Random;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	internal Action<AcceleratorStream, Index1D, ArrayView<int>, RNGView<XorShift128Plus>, int, long> fillUniformInt
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(fillUniformInt), KernelDomain.Generators);

	internal void LoadGeneratorsInt32Kernels() =>
		fillUniformInt = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<int>, RNGView<XorShift128Plus>, int, long>(FillUniformInt_Kern);

	static void FillUniformInt_Kern(
		Index1D index,
		ArrayView<int> output,
		RNGView<XorShift128Plus> rng,
		int minimum,
		long range) =>
		output[index] = (int)(minimum + (long)(rng.NextDouble() * range));
}
