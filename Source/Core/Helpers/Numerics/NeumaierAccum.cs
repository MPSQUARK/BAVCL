using System;
using System.Numerics;

namespace BAVCL.Core.Helpers.Numerics;

/// <summary>Production float32 sum/dot via widened SIMD Neumaier accumulation in float64. See citations.md [9].</summary>
internal static class NeumaierAccum
{
	internal static float Sum(ReadOnlySpan<float> data)
	{
		if (data.IsEmpty)
			return 0f;

		int lanes = SimdMath.FloatLanes;
		SimdFloat64NeumaierState lower = SimdFloat64NeumaierState.Zero;
		SimdFloat64NeumaierState upper = SimdFloat64NeumaierState.Zero;
		Float64NeumaierState total = Float64NeumaierState.Zero;
		int index = 0;

		for (; index <= data.Length - lanes; index += lanes)
		{
			Vector<float> chunk = new(data.Slice(index, lanes));
			SimdMath.WidenToDoubleHalves(chunk, out Vector<double> lowerInput, out Vector<double> upperInput);
			lower.Add(lowerInput);
			upper.Add(upperInput);
		}

		lower.FoldInto(ref total);
		upper.FoldInto(ref total);

		for (; index < data.Length; index++)
			total.Add(data[index]);

		return total.ToSingle();
	}

	internal static float Dot(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
	{
		if (left.IsEmpty)
			return 0f;

		int lanes = SimdMath.FloatLanes;
		SimdFloat64NeumaierState lower = SimdFloat64NeumaierState.Zero;
		SimdFloat64NeumaierState upper = SimdFloat64NeumaierState.Zero;
		Float64NeumaierState total = Float64NeumaierState.Zero;
		int index = 0;

		for (; index <= left.Length - lanes; index += lanes)
		{
			Vector<float> product = new Vector<float>(left.Slice(index, lanes)) * new Vector<float>(right.Slice(index, lanes));
			SimdMath.WidenToDoubleHalves(product, out Vector<double> lowerInput, out Vector<double> upperInput);
			lower.Add(lowerInput);
			upper.Add(upperInput);
		}

		lower.FoldInto(ref total);
		upper.FoldInto(ref total);

		for (; index < left.Length; index++)
			total.Add(left[index] * right[index]);

		return total.ToSingle();
	}
}
