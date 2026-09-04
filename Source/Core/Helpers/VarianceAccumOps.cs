using System;
using System.Numerics;
using BAVCL.Core.Helpers.Numerics;

namespace BAVCL.Core.Helpers;

/// <summary>CPU population-variance entry points. See citations.md [2].</summary>
internal static class VarianceAccumOps
{
	/// <summary>Welford single-element update. Used for tails and GPU host fold.</summary>
	internal static void Update(ref int count, ref float mean, ref float m2, float value)
	{
		VarianceMoments moments = new(count, mean, m2);
		moments.AddSample(value);
		count = moments.Count;
		mean = moments.Mean;
		m2 = moments.M2;
	}

	/// <summary>Chan–Golub–LeVeque merge. Used for GPU host fold.</summary>
	internal static void Merge(ref int countA, ref float meanA, ref float m2A, int countB, float meanB, float m2B)
	{
		VarianceMoments left = new(countA, meanA, m2A);
		left.MergeWith(new VarianceMoments(countB, meanB, m2B));
		countA = left.Count;
		meanA = left.Mean;
		m2A = left.M2;
	}

	internal static float FromAccumulators(int count, float m2) =>
		count == 0 ? 0f : m2 / count;
	/// <summary>SIMD block moments merged with streaming Chan–Golub–LeVeque.</summary>
	internal static float PopulationVariance(ReadOnlySpan<float> data)
	{
		if (data.IsEmpty)
			return 0f;

		int lanes = SimdMath.FloatLanes;
		VarianceMoments total = default;
		int index = 0;

		for (; index <= data.Length - lanes; index += lanes)
		{
			Vector<float> chunk = new(data.Slice(index, lanes));
			total.MergeWith(VarianceMoments.FromSimdBlock(chunk));
		}

		for (; index < data.Length; index++)
			total.AddSample(data[index]);

		return total.PopulationVariance();
	}

	internal static float PopulationVariance(ReadOnlySpan<int> data)
	{
		if (data.IsEmpty)
			return 0f;

		int lanes = SimdMath.FloatLanes;
		Span<float> intToFloatBuffer = stackalloc float[lanes];
		VarianceMoments total = default;
		int index = 0;

		for (; index <= data.Length - lanes; index += lanes)
		{
			Vector<int> intChunk = new(data.Slice(index, lanes));
			Vector<float> chunk = SimdMath.ConvertInt32ToFloat(intChunk, intToFloatBuffer);
			total.MergeWith(VarianceMoments.FromSimdBlock(chunk));
		}

		for (; index < data.Length; index++)
			total.AddSample(data[index]);

		return total.PopulationVariance();
	}
}
