using System;
using System.Collections.Generic;
using BAVCL.Core;

namespace BAVCL.Modules.Sorting;

internal static class SortCpuSegment
{
	internal static void Sort<T>(T[] values, int offset, int count, SortOrder order) where T : IComparable<T>
	{
		if (count <= 1)
			return;

		if (order == SortOrder.Ascending)
		{
			Array.Sort(values, offset, count);
			return;
		}

		Array.Sort(values, offset, count, DescendingComparer<T>.Instance);
	}

	static class DescendingComparer<T> where T : IComparable<T>
	{
		internal static readonly Comparer<T> Instance =
			Comparer<T>.Create(static (a, b) => b.CompareTo(a));
	}
}
