namespace BAVCL.Modules.Sorting;

internal static class SortCore
{
	internal static VectorInt Sort(VectorInt vector, SortOrder order, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		SortIP(copy, order, upload);
		return copy;
	}

	internal static VectorInt SortAsc(VectorInt vector, Upload upload = Upload.Now) => Sort(vector, SortOrder.Ascending, upload);

	internal static VectorInt SortDesc(VectorInt vector, Upload upload = Upload.Now) => Sort(vector, SortOrder.Descending, upload);

	internal static Vector Sort(Vector vector, SortOrder order, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		SortIP(copy, order, upload);
		return copy;
	}

	internal static Vector SortAsc(Vector vector, Upload upload = Upload.Now) => Sort(vector, SortOrder.Ascending, upload);

	internal static Vector SortDesc(Vector vector, Upload upload = Upload.Now) => Sort(vector, SortOrder.Descending, upload);

	internal static void SortAscIP(VectorInt vector, Upload upload = Upload.Now) => SortIP(vector, SortOrder.Ascending, upload);

	internal static void SortDescIP(VectorInt vector, Upload upload = Upload.Now) => SortIP(vector, SortOrder.Descending, upload);

	internal static void SortAscIP(Vector vector, Upload upload = Upload.Now) => SortIP(vector, SortOrder.Ascending, upload);

	internal static void SortDescIP(Vector vector, Upload upload = Upload.Now) => SortIP(vector, SortOrder.Descending, upload);

	internal static void SortIP(VectorInt vector, SortOrder order, Upload upload = Upload.Now)
	{
		if (vector.Length <= 1)
			return;

		using (vector.CpuScope(upload))
		{
			if (vector.Is1D())
			{
				SortCpuRows.Sort1D(vector.Value, order);
				return;
			}

			SortCpuRows.Sort2D(vector.Value, vector.ElementsPerRow(), vector.RowCount(), order);
		}
	}

	internal static void SortIP(Vector vector, SortOrder order, Upload upload = Upload.Now)
	{
		if (vector.Length <= 1)
			return;

		using (vector.CpuScope(upload))
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
