using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms.Random;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	internal Action<AcceleratorStream, Index1D, ArrayView<float>, float, float> fillSequence
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(fillSequence), KernelDomain.Generators);

	internal Action<AcceleratorStream, Index1D, ArrayView<float>, RNGView<XorShift128Plus>, float, float> fillUniform
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(fillUniform), KernelDomain.Generators);

	internal void LoadGeneratorsFloat32Kernels()
	{
		fillSequence = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, float>(FillSequence_Kern);
		fillUniform = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<float>, RNGView<XorShift128Plus>, float, float>(FillUniform_Kern);
	}

	static void FillSequence_Kern(Index1D index, ArrayView<float> output, float start, float step) =>
		output[index] = start + index * step;

	static void FillUniform_Kern(
		Index1D index,
		ArrayView<float> output,
		RNGView<XorShift128Plus> rng,
		float minimum,
		float maximum)
	{
		float random = rng.NextFloat();
		output[index] = minimum * (1f - random) + maximum * random;
	}
}
