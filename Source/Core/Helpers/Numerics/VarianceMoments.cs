using System;

namespace BAVCL.Core.Helpers.Numerics;

/// <summary>Welford / Chan–Golub–LeVeque variance state (count, mean, M2). See citations.md [2].</summary>
internal struct VarianceMoments
{
	public int Count;
	public float Mean;
	public float M2;

	public void Add(float value) => VarianceAccum.Update(ref Count, ref Mean, ref M2, value);

	public void Merge(in VarianceMoments other) =>
		VarianceAccum.Merge(ref Count, ref Mean, ref M2, other.Count, other.Mean, other.M2);

	public static VarianceMoments MergeGroupedPartials(ReadOnlySpan<float> countMeanM2Triplets)
	{
		VarianceMoments total = default;
		for (int i = 0; i + 2 < countMeanM2Triplets.Length; i += 3)
		{
			VarianceMoments block = new()
			{
				Count = (int)countMeanM2Triplets[i],
				Mean = countMeanM2Triplets[i + 1],
				M2 = countMeanM2Triplets[i + 2],
			};
			total.Merge(in block);
		}

		return total;
	}
}
