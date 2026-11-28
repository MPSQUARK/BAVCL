using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	internal Action<AcceleratorStream, Index1D, ArrayView<float>, float, float> fillSequence
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(fillSequence), KernelDomain.Generators);

	internal Action<AcceleratorStream, Index1D, ArrayView<float>, float, float, float> scaleUniform
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(scaleUniform), KernelDomain.Generators);

	internal Action<AcceleratorStream, Index1D, ArrayView<float>, float, float> normalScale
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(normalScale), KernelDomain.Generators);

	internal void LoadGeneratorsFloat32Kernels()
	{
		fillSequence = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, float>(FillSequence_Kern);
		scaleUniform = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, float, float>(ScaleUniform_Kern);
		normalScale = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, float>(NormalScale_Kern);
	}

	static void FillSequence_Kern(Index1D index, ArrayView<float> output, float start, float step) =>
		output[index] = start + index * step;

	/// <summary>
	/// Maps ILGPU's uniform samples in [0, 1), held in <paramref name="output"/>, onto
	/// [<paramref name="minimum"/>, <paramref name="maximum"/>) in place. Rounding can land exactly on
	/// <paramref name="maximum"/>, so results are capped at <paramref name="belowMaximum"/>, the largest
	/// float below it.
	/// </summary>
	static void ScaleUniform_Kern(
		Index1D index, ArrayView<float> output, float minimum, float maximum, float belowMaximum)
	{
		float unit = output[index];
		float scaled = minimum * (1f - unit) + maximum * unit;
		output[index] = XMath.Min(scaled, belowMaximum);
	}

	/// <summary>
	/// Maps ILGPU's uniform samples in [0, 1), held in <paramref name="output"/>, onto a normal
	/// distribution with the given mean and standard deviation in place, by inverse-CDF transform.
	/// Samples are clamped to [2^-24, 1 - 2^-24] so the transform stays finite. That truncates
	/// the tails beyond about 5.3 standard deviations.
	/// </summary>
	static void NormalScale_Kern(Index1D index, ArrayView<float> output, float mean, float stdDev)
	{
		const float Lower = 1f / 16777216f;
		float p = output[index];
		if (p < Lower)
			p = Lower;
		if (p > 1f - Lower)
			p = 1f - Lower;
		output[index] = mean + stdDev * StandardNormalQuantile(p);
	}

	/// <summary>
	/// Standard normal quantile for p in (0, 1), using Acklam's rational approximation (see citations.md [12]).
	/// The relative error is about 1.2e-9 in double precision, and single precision rounding dominates here.
	/// </summary>
	static float StandardNormalQuantile(float p)
	{
		const float PLow = 0.02425f;
		if (p < PLow)
			return TailQuantile(XMath.Sqrt(-2f * XMath.Log(p)));
		if (p > 1f - PLow)
			return -TailQuantile(XMath.Sqrt(-2f * XMath.Log(1f - p)));

		float q = p - 0.5f;
		float r = q * q;
		float numerator = ((((-3.969683028665376e+01f * r + 2.209460984245205e+02f) * r
			- 2.759285104469687e+02f) * r + 1.383577518672690e+02f) * r - 3.066479806614716e+01f) * r + 2.506628277459239e+00f;
		float denominator = ((((-5.447609879822406e+01f * r + 1.615858368580409e+02f) * r
			- 1.556989798598866e+02f) * r + 6.680131188771972e+01f) * r - 1.328068155288572e+01f) * r + 1f;
		return numerator * q / denominator;
	}

	/// <summary>Acklam's tail rational function, evaluated at q = sqrt(-2 ln p) for p in the lower tail.</summary>
	static float TailQuantile(float q)
	{
		float numerator = ((((-7.784894002430293e-03f * q - 3.223964580411365e-01f) * q
			- 2.400758277161838e+00f) * q - 2.549732539343734e+00f) * q + 4.374664141464968e+00f) * q + 2.938163982698783e+00f;
		float denominator = (((7.784695709041462e-03f * q + 3.224671290700398e-01f) * q
			+ 2.445134137142996e+00f) * q + 3.754408661907416e+00f) * q + 1f;
		return numerator / denominator;
	}
}
