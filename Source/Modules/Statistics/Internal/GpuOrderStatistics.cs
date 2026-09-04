using System;
using BAVCL.Core.Helpers;
using BAVCL.Modules.Sorting;

namespace BAVCL.Modules.Statistics;

// GPU order stats sort on device, then read sorted elements on CPU (linear interpolation).
internal static class GpuOrderStatistics
{
	internal static float PercentileX(Vector vector, float percentile)
	{
		Guard.IsInRangeInclusive(percentile, Guard.PercentileMin, Guard.PercentileMax);
		Vector sorted = vector.SortAscX();
		return OrderStatistics.Percentile(sorted.RetrieveReadOnlySpan(), percentile);
	}

	internal static float PercentileX(VectorInt vector, float percentile)
	{
		Guard.IsInRangeInclusive(percentile, Guard.PercentileMin, Guard.PercentileMax);
		VectorInt sorted = vector.SortAscX();
		return OrderStatistics.Percentile(sorted.RetrieveReadOnlySpan(), percentile);
	}

	internal static float MedianX(Vector vector) => PercentileX(vector, 50f);

	internal static float MedianX(VectorInt vector) => PercentileX(vector, 50f);

	internal static float Quartile1X(Vector vector) => PercentileX(vector, 25f);

	internal static float Quartile1X(VectorInt vector) => PercentileX(vector, 25f);

	internal static float Quartile3X(Vector vector) => PercentileX(vector, 75f);

	internal static float Quartile3X(VectorInt vector) => PercentileX(vector, 75f);

	internal static float IqrX(Vector vector) => IqrFromSortedX(vector.SortAscX());

	internal static float IqrX(VectorInt vector) => IqrFromSortedX(vector.SortAscX());

	static float IqrFromSortedX(Vector sorted)
	{
		ReadOnlySpan<float> data = sorted.RetrieveReadOnlySpan();
		return OrderStatistics.Percentile(data, 75f) - OrderStatistics.Percentile(data, 25f);
	}

	static float IqrFromSortedX(VectorInt sorted)
	{
		ReadOnlySpan<int> data = sorted.RetrieveReadOnlySpan();
		return OrderStatistics.Percentile(data, 75f) - OrderStatistics.Percentile(data, 25f);
	}
}
