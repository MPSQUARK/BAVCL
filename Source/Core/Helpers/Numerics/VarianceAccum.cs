using System;
using System.Numerics;
using SimdVector = System.Numerics.Vector;

namespace BAVCL.Core.Helpers.Numerics;

/// <summary>
/// CPU variance accumulation (Welford + SIMD blocks + Chan merge).
/// GPU grouped variance kernels emit per-block (count, mean, M2) triplets; host merges via <see cref="VarianceMoments.MergeGroupedPartials"/>.
/// See citations.md [2], [11].
/// </summary>
internal static class VarianceAccum
{
	/// <summary>Welford single-element update.</summary>
	internal static void Update(ref int count, ref float mean, ref float m2, float value)
	{
		count++;
		float invCount = 1f / count;
		float delta = value - mean;
		mean += delta * invCount;
		m2 += delta * (value - mean);
	}

	/// <summary>Chan–Golub–LeVeque merge.</summary>
	internal static void Merge(
		ref int countA,
		ref float meanA,
		ref float m2A,
		int countB,
		float meanB,
		float m2B)
	{
		if (countB == 0)
			return;

		if (countA == 0)
		{
			countA = countB;
			meanA = meanB;
			m2A = m2B;
			return;
		}

		int mergedCount = countA + countB;
		float invMergedCount = 1f / mergedCount;
		float delta = meanB - meanA;
		m2A = m2A + m2B + delta * delta * countA * countB * invMergedCount;
		meanA += delta * countB * invMergedCount;
		countA = mergedCount;
	}

	internal static float PopulationVarianceFromMoments(int count, float m2) =>
		count == 0 ? 0f : m2 / count;

	internal static float SampleVarianceFromMoments(int count, float m2)
	{
		if (count == 0)
			return 0f;

		Guard.IsNotZero(count - 1);
		return m2 / (count - 1);
	}

	internal static float PopulationVariance(ReadOnlySpan<float> data)
	{
		if (data.IsEmpty)
			return 0f;

		Accumulate(data, out int count, out _, out float m2);
		return PopulationVarianceFromMoments(count, m2);
	}

	internal static float SampleVariance(ReadOnlySpan<float> data)
	{
		if (data.IsEmpty)
			return 0f;

		Accumulate(data, out int count, out _, out float m2);
		return SampleVarianceFromMoments(count, m2);
	}

	internal static float PopulationVariance(ReadOnlySpan<int> data)
	{
		if (data.IsEmpty)
			return 0f;

		Accumulate(data, out int count, out _, out float m2);
		return PopulationVarianceFromMoments(count, m2);
	}

	internal static float SampleVariance(ReadOnlySpan<int> data)
	{
		if (data.IsEmpty)
			return 0f;

		Accumulate(data, out int count, out _, out float m2);
		return SampleVarianceFromMoments(count, m2);
	}

	internal static void Accumulate(ReadOnlySpan<float> data, out int count, out float mean, out float m2)
	{
		count = 0;
		mean = 0f;
		m2 = 0f;

		int lanes = Vector<float>.Count;
		int index = 0;

		for (; index <= data.Length - lanes; index += lanes)
		{
			Vector<float> chunk = new(data.Slice(index, lanes));
			MergeSimdBlock(ref count, ref mean, ref m2, chunk);
		}

		for (; index < data.Length; index++)
			Update(ref count, ref mean, ref m2, data[index]);
	}

	internal static void Accumulate(ReadOnlySpan<int> data, out int count, out float mean, out float m2)
	{
		count = 0;
		mean = 0f;
		m2 = 0f;

		int lanes = Vector<float>.Count;
		int index = 0;

		for (; index <= data.Length - lanes; index += lanes)
		{
			Vector<int> intChunk = new(data.Slice(index, lanes));
			Vector<float> chunk = SimdVector.ConvertToSingle(intChunk);
			MergeSimdBlock(ref count, ref mean, ref m2, chunk);
		}

		for (; index < data.Length; index++)
			Update(ref count, ref mean, ref m2, data[index]);
	}

	static void MergeSimdBlock(ref int count, ref float mean, ref float m2, Vector<float> values)
	{
		int blockCount = Vector<float>.Count;
		float invBlockCount = 1f / blockCount;
		float blockMean = SimdVector.Sum(values) * invBlockCount;
		Vector<float> centered = values - new Vector<float>(blockMean);
		float blockM2 = SimdVector.Dot(centered, centered);
		Merge(ref count, ref mean, ref m2, blockCount, blockMean, blockM2);
	}
}
