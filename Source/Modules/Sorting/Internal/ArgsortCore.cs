using System;
using System.Collections.Generic;

namespace BAVCL.Modules.Sorting;

internal static class ArgsortCore
{
	internal static VectorInt ArgsortAsc(VectorInt vector, Upload upload = Upload.Now) =>
		Argsort(vector, SortOrder.Ascending, upload);

	internal static VectorInt ArgsortDesc(VectorInt vector, Upload upload = Upload.Now) =>
		Argsort(vector, SortOrder.Descending, upload);

	internal static VectorInt ArgsortAsc(Vector vector, Upload upload = Upload.Now) =>
		Argsort(vector, SortOrder.Ascending, upload);

	internal static VectorInt ArgsortDesc(Vector vector, Upload upload = Upload.Now) =>
		Argsort(vector, SortOrder.Descending, upload);

	internal static VectorInt Argsort(VectorInt vector, SortOrder order, Upload upload = Upload.Now) =>
		BuildArgsortVector(vector, vector.RetrieveReadOnlySpan().ToArray(), order, upload);

	internal static VectorInt Argsort(Vector vector, SortOrder order, Upload upload = Upload.Now) =>
		BuildArgsortVector(vector, vector.RetrieveReadOnlySpan().ToArray(), order, upload);

	internal static void ArgsortAscIP(VectorInt vector, VectorInt indices, Upload upload = Upload.Now) =>
		ArgsortIP(vector, indices, SortOrder.Ascending, upload);

	internal static void ArgsortDescIP(VectorInt vector, VectorInt indices, Upload upload = Upload.Now) =>
		ArgsortIP(vector, indices, SortOrder.Descending, upload);

	internal static void ArgsortAscIP(Vector vector, VectorInt indices, Upload upload = Upload.Now) =>
		ArgsortIP(vector, indices, SortOrder.Ascending, upload);

	internal static void ArgsortDescIP(Vector vector, VectorInt indices, Upload upload = Upload.Now) =>
		ArgsortIP(vector, indices, SortOrder.Descending, upload);

	internal static void ArgsortIP(VectorInt vector, VectorInt indices, SortOrder order, Upload upload = Upload.Now) =>
		BuildArgsortInto(vector, indices, vector.RetrieveReadOnlySpan().ToArray(), order, upload);

	internal static void ArgsortIP(Vector vector, VectorInt indices, SortOrder order, Upload upload = Upload.Now) =>
		BuildArgsortInto(vector, indices, vector.RetrieveReadOnlySpan().ToArray(), order, upload);

	static VectorInt BuildArgsortVector<T>(VectorBase<T> vector, T[] values, SortOrder order, Upload upload)
		where T : unmanaged, IComparable<T>
	{
		if (vector.Length == 0)
			return new VectorInt(0, vector.Columns, gpu: vector.Gpu);

		int[] indices = new int[vector.Length];

		if (vector.Is1D())
		{
			BuildRow(values, indices, offset: 0, count: values.Length, order);
			return new VectorInt(indices, vector.Columns, upload, vector.Gpu);
		}

		int cols = vector.ElementsPerRow();
		SortCpuParallelRows.ForEachRow(vector.RowCount(), cols, row =>
			BuildRow(values, indices, row * cols, cols, order));
		return new VectorInt(indices, vector.Columns, upload, vector.Gpu);
	}

	static void BuildArgsortInto<T>(VectorBase<T> vector, VectorInt indices, T[] values, SortOrder order, Upload upload)
		where T : unmanaged, IComparable<T>
	{
        Guard.MatchingGpuPairIndices(vector, indices);

        if (vector.Length == 0)
			return;

		// ArgsortIP always fully overwrites every element, so a stale/empty CPU-side buffer (e.g. a
		// freshly GPU-allocated `indices`) is fine — presize it here rather than relying on
		// CpuScope's sync-on-enter, which never pulls once residence flips to CPU-authoritative.
		if (indices.Value.Length != indices.Length)
			indices.Value = new int[indices.Length];

		using (indices.CpuScope(upload))
		{
			int[] target = indices.Value;

			if (vector.Is1D())
			{
				BuildRow(values, target, offset: 0, count: values.Length, order);
				return;
			}

			int cols = vector.ElementsPerRow();
			SortCpuParallelRows.ForEachRow(vector.RowCount(), cols, row =>
				BuildRow(values, target, row * cols, cols, order));
		}
	}

	static void BuildRow<T>(T[] values, int[] indices, int offset, int count, SortOrder order) where T : IComparable<T>
	{
		SortIndexFill.WriteRowLocal(indices, offset, count);
		Array.Sort(indices, offset, count, Comparer<int>.Create((a, b) =>
			CompareByOrder(values[offset + a], values[offset + b], order)));
	}

	static int CompareByOrder<T>(T left, T right, SortOrder order) where T : IComparable<T>
	{
		int cmp = left.CompareTo(right);
		// SortOrder: Ascending=0, Descending=-1 — cast is the flip mask (0 or all-ones).
		int flip = (int)order;
		return (cmp ^ flip) - flip;
	}
}
