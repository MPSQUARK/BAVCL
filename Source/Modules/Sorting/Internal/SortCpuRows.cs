using System;
using BAVCL.Core;

namespace BAVCL.Modules.Sorting;

internal static class SortCpuRows
{
	internal static void Sort1D<T>(T[] values, SortOrder order) where T : IComparable<T> =>
		SortCpuSegment.Sort(values, offset: 0, values.Length, order);

	internal static void Sort2D<T>(T[] values, int cols, int rowCount, SortOrder order) where T : IComparable<T>
	{
		SortCpuParallelRows.ForEachRow(rowCount, cols, row =>
			SortCpuSegment.Sort(values, row * cols, cols, order));
	}
}
