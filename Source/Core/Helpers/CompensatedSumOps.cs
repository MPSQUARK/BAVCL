using System;
using BAVCL.Core.Helpers.Numerics;

namespace BAVCL.Core.Helpers;

/// <summary>CPU compensated summation entry points. See citations.md [9].</summary>
internal static class CompensatedSumOps
{
	/// <inheritdoc cref="NeumaierAccum.Sum"/>
	internal static float SumNeumaier(ReadOnlySpan<float> data) =>
		NeumaierAccum.Sum(data);

	/// <inheritdoc cref="NeumaierAccum.Dot"/>
	internal static float DotNeumaier(ReadOnlySpan<float> left, ReadOnlySpan<float> right) =>
		NeumaierAccum.Dot(left, right);

	/// <summary>Host/GPU fold helper — scalar float64 Neumaier step.</summary>
	internal static void NeumaierAdd(ref double sum, ref double compensation, float input) =>
		Float64NeumaierState.Step(ref sum, ref compensation, input);
}
