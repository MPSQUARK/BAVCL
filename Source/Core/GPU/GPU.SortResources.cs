using System;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public sealed partial class GPU
{
	/// <summary>
	/// Converts a segment length to an ILGPU launch extent, guarding against lengths beyond
	/// <see cref="Index1D"/>'s int range instead of relying on an implicit narrowing cast.
	/// </summary>
	internal static Index1D SortLaunchExtent(long length)
	{
		if (length > int.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(length), length, "Sort segment length exceeds Index1D range.");

		return checked((int)length);
	}

	internal static (ArrayView<int> keysTemp, ArrayView<int> valuesTemp) SegmentedSortTempViews(
		Core.CacheableBase<int> temp,
		int length,
		SortTempLayout layout)
	{
		var view = temp.GetBuffer().View;
		return layout == SortTempLayout.KeysAndValues
			? (view.SubView(0, length), view.SubView(length, length))
			: (view.SubView(0, length), default);
	}
}
