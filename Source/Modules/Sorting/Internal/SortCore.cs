using BAVCL.Core;

namespace BAVCL.Modules.Sorting;

internal static class SortCore
{
	internal static VectorInt Sort(VectorInt vector, SortOrder order, bool syncToGpu = true)
	{
		VectorInt copy = vector.Copy();
		SortIP(copy, order, syncToGpu);
		return copy;
	}

	internal static VectorInt SortAsc(VectorInt vector, bool syncToGpu = true) => Sort(vector, SortOrder.Ascending, syncToGpu);

	internal static VectorInt SortDesc(VectorInt vector, bool syncToGpu = true) => Sort(vector, SortOrder.Descending, syncToGpu);

	internal static Vector Sort(Vector vector, SortOrder order, bool syncToGpu = true)
	{
		Vector copy = vector.Copy();
		SortIP(copy, order, syncToGpu);
		return copy;
	}

	internal static Vector SortAsc(Vector vector, bool syncToGpu = true) => Sort(vector, SortOrder.Ascending, syncToGpu);

	internal static Vector SortDesc(Vector vector, bool syncToGpu = true) => Sort(vector, SortOrder.Descending, syncToGpu);

	internal static void SortAscIP(VectorInt vector, bool syncToGpu = true) => SortIP(vector, SortOrder.Ascending, syncToGpu);

	internal static void SortDescIP(VectorInt vector, bool syncToGpu = true) => SortIP(vector, SortOrder.Descending, syncToGpu);

	internal static void SortAscIP(Vector vector, bool syncToGpu = true) => SortIP(vector, SortOrder.Ascending, syncToGpu);

	internal static void SortDescIP(Vector vector, bool syncToGpu = true) => SortIP(vector, SortOrder.Descending, syncToGpu);

	internal static void SortIP(VectorInt vector, SortOrder order, bool syncToGpu = true)
	{
		if (vector.Length <= 1)
			return;

		using (vector.CpuScope(syncToGpu))
		{
			if (vector.Is1D())
			{
				SortCpuRows.Sort1D(vector.Value, order);
				return;
			}

			SortCpuRows.Sort2D(vector.Value, vector.ElementsPerRow(), vector.RowCount(), order);
		}
	}

	internal static void SortIP(Vector vector, SortOrder order, bool syncToGpu = true)
	{
		if (vector.Length <= 1)
			return;

		using (vector.CpuScope(syncToGpu))
		{
			if (vector.Is1D())
			{
				SortCpuRows.Sort1D(vector.Value, order);
				return;
			}

			SortCpuRows.Sort2D(vector.Value, vector.ElementsPerRow(), vector.RowCount(), order);
		}
	}
}
