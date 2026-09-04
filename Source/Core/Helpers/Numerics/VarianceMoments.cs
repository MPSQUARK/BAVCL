using System.Numerics;

namespace BAVCL.Core.Helpers.Numerics;

/// <summary>Population-variance sufficient statistics. See citations.md [11], [2].</summary>
internal struct VarianceMoments(int count, float mean, float m2)
{
	public int Count = count;
	public float Mean = mean;
	public float M2 = m2;
	public float InvCount = 1f / count;

	/// <summary>Welford single-element update.</summary>
	public void AddSample(float value)
	{
		Count++;
		InvCount = 1f / Count;
		float delta = value - Mean;
		Mean += delta * InvCount;
		M2 += delta * (value - Mean);
	}

	/// <summary>
	/// Chan–Golub–LeVeque merge. <paramref name="other"/> is typically a fixed-width SIMD block
	/// merged into a growing running total.
	/// </summary>
	public void MergeWith(in VarianceMoments other)
	{
		if (other.Count == 0)
			return;

		if (Count == 0)
		{
			Count = other.Count;
			Mean = other.Mean;
			M2 = other.M2;
			InvCount = other.InvCount;
			return;
		}

		int mergedCount = Count + other.Count;
		float invMergedCount = 1f / mergedCount;
		float delta = other.Mean - Mean;
		M2 = M2 + other.M2 + delta * delta * Count * other.Count * invMergedCount;
		Mean += delta * other.Count * invMergedCount;
		Count = mergedCount;
		InvCount = invMergedCount;
	}

	public readonly float PopulationVariance()
	{
		if (Count == 0)
			return 0f;

		return M2 * InvCount;
	}

	/// <summary>Unbiased sample variance (s²): Σ(x−x̄)² / (N−1). Requires <see cref="Count"/> ≥ 2.</summary>
	public readonly float SampleVariance()
	{
		if (Count == 0)
			return 0f;

		Guard.IsNotZero(Count - 1);
		return M2 / (Count - 1);
	}

	/// <summary>Mean and M2 of one contiguous SIMD chunk (vectorized center + square).</summary>
	public static VarianceMoments FromSimdBlock(Vector<float> values)
	{
		int blockCount = Vector<float>.Count;
		float invBlockCount = 1f / blockCount;
		float blockMean = SimdMath.HorizontalSum(values) * invBlockCount;
		Vector<float> centered = values - new Vector<float>(blockMean);
		float blockM2 = SimdMath.HorizontalSum(centered * centered);
		return new VarianceMoments(blockCount, blockMean, blockM2);
	}
}
