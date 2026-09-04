using System;
using BAVCL.Core.Helpers;
using BAVCL.Modules.Sorting;

namespace BAVCL.Modules.Statistics;

internal static class OrderStatistics
{
	internal static float Percentile(Vector vector, float percentile)
	{
		Guard.IsInRangeInclusive(percentile, Guard.PercentileMin, Guard.PercentileMax);
		return Percentile(vector.SortAsc().RetrieveReadOnlySpan(), percentile);
	}

	internal static float Percentile(VectorInt vector, float percentile)
	{
		Guard.IsInRangeInclusive(percentile, Guard.PercentileMin, Guard.PercentileMax);
		return Percentile(vector.SortAsc().RetrieveReadOnlySpan(), percentile);
	}

	internal static float Median(Vector vector) => PercentileFromSorted(vector.SortAsc(), 50f);

	internal static float Median(VectorInt vector) => PercentileFromSorted(vector.SortAsc(), 50f);

	internal static float Quartile1(Vector vector) => PercentileFromSorted(vector.SortAsc(), 25f);

	internal static float Quartile1(VectorInt vector) => PercentileFromSorted(vector.SortAsc(), 25f);

	internal static float Quartile3(Vector vector) => PercentileFromSorted(vector.SortAsc(), 75f);

	internal static float Quartile3(VectorInt vector) => PercentileFromSorted(vector.SortAsc(), 75f);

	internal static float Iqr(Vector vector)
	{
		ReadOnlySpan<float> sorted = vector.SortAsc().RetrieveReadOnlySpan();
		return Percentile(sorted, 75f) - Percentile(sorted, 25f);
	}

	internal static float Iqr(VectorInt vector)
	{
		ReadOnlySpan<int> sorted = vector.SortAsc().RetrieveReadOnlySpan();
		return Percentile(sorted, 75f) - Percentile(sorted, 25f);
	}

	internal static float Percentile(ReadOnlySpan<float> sorted, float percentile)
	{
		Guard.IsNotEmpty(sorted.Length);
		Guard.IsInRangeInclusive(percentile, Guard.PercentileMin, Guard.PercentileMax);

		if (sorted.Length == 1)
			return sorted[0];

		(int lower, int upper, float weight) = RankIndices(sorted.Length, percentile);
		return sorted[lower] + weight * (sorted[upper] - sorted[lower]);
	}

	internal static float Percentile(ReadOnlySpan<int> sorted, float percentile)
	{
		Guard.IsNotEmpty(sorted.Length);
		Guard.IsInRangeInclusive(percentile, Guard.PercentileMin, Guard.PercentileMax);

		if (sorted.Length == 1)
			return sorted[0];

		(int lower, int upper, float weight) = RankIndices(sorted.Length, percentile);
		return sorted[lower] + weight * (sorted[upper] - sorted[lower]);
	}

	static float PercentileFromSorted(Vector sorted, float percentile) =>
		Percentile(sorted.RetrieveReadOnlySpan(), percentile);

	static float PercentileFromSorted(VectorInt sorted, float percentile) =>
		Percentile(sorted.RetrieveReadOnlySpan(), percentile);

	static (int lower, int upper, float weight) RankIndices(int length, float percentile)
	{
		float rank = (length - 1) * percentile / 100f;
		int lower = (int)MathF.Floor(rank);
		int upper = (int)MathF.Ceiling(rank);
		return (lower, upper, rank - lower);
	}
}
