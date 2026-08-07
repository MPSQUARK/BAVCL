using System;
using System.Threading.Tasks;

namespace BAVCL.Modules.Sorting;

internal static class SortCpuParallelRows
{
	internal const int MinParallelRows = 4;
	internal const int MinParallelElements = 100_000;

	internal static bool ShouldParallelize(int rowCount, int cols) =>
		rowCount >= MinParallelRows && rowCount * cols >= MinParallelElements;

	internal static void ForEachRow(int rowCount, int cols, Action<int> processRow)
	{
		if (!ShouldParallelize(rowCount, cols))
		{
			for (int row = 0; row < rowCount; row++)
				processRow(row);
			return;
		}

		Parallel.For(0, rowCount, processRow);
	}
}
