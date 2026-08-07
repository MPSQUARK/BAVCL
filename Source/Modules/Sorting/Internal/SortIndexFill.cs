namespace BAVCL.Modules.Sorting;

/// <summary>Fills an index buffer segment with local (0-based, row-relative) indices.</summary>
internal static class SortIndexFill
{
	internal static void WriteRowLocal(int[] indices, int offset, int count)
	{
		for (int i = 0; i < count; i++)
			indices[offset + i] = i;
	}
}
