using System;
using System.Collections.Generic;
using BAVCL.Core;
using BAVCL.Core.Exceptions;

namespace BAVCL.Modules.Sorting;

internal static class ArgsortCore
{
	internal static VectorInt ArgsortAsc(VectorInt vector, bool syncToGpu = true) =>
		Argsort(vector, SortOrder.Ascending, syncToGpu);

	internal static VectorInt ArgsortDesc(VectorInt vector, bool syncToGpu = true) =>
		Argsort(vector, SortOrder.Descending, syncToGpu);

	internal static VectorInt ArgsortAsc(Vector vector, bool syncToGpu = true) =>
		Argsort(vector, SortOrder.Ascending, syncToGpu);

	internal static VectorInt ArgsortDesc(Vector vector, bool syncToGpu = true) =>
		Argsort(vector, SortOrder.Descending, syncToGpu);

	internal static VectorInt Argsort(VectorInt vector, SortOrder order, bool syncToGpu = true) =>
		BuildArgsortVector(vector, vector.RetrieveReadOnlySpan().ToArray(), order, syncToGpu);

	internal static VectorInt Argsort(Vector vector, SortOrder order, bool syncToGpu = true) =>
		BuildArgsortVector(vector, vector.RetrieveReadOnlySpan().ToArray(), order, syncToGpu);

	internal static void ArgsortAscIP(VectorInt vector, VectorInt indices, bool syncToGpu = true) =>
		ArgsortIP(vector, indices, SortOrder.Ascending, syncToGpu);

	internal static void ArgsortDescIP(VectorInt vector, VectorInt indices, bool syncToGpu = true) =>
		ArgsortIP(vector, indices, SortOrder.Descending, syncToGpu);

	internal static void ArgsortAscIP(Vector vector, VectorInt indices, bool syncToGpu = true) =>
		ArgsortIP(vector, indices, SortOrder.Ascending, syncToGpu);

	internal static void ArgsortDescIP(Vector vector, VectorInt indices, bool syncToGpu = true) =>
		ArgsortIP(vector, indices, SortOrder.Descending, syncToGpu);

	internal static void ArgsortIP(VectorInt vector, VectorInt indices, SortOrder order, bool syncToGpu = true) =>
		BuildArgsortInto(vector, indices, vector.RetrieveReadOnlySpan().ToArray(), order, syncToGpu);

	internal static void ArgsortIP(Vector vector, VectorInt indices, SortOrder order, bool syncToGpu = true) =>
		BuildArgsortInto(vector, indices, vector.RetrieveReadOnlySpan().ToArray(), order, syncToGpu);

	static VectorInt BuildArgsortVector<T>(VectorBase<T> vector, T[] values, SortOrder order, bool syncToGpu)
		where T : unmanaged, IComparable<T>
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		int[] indices = new int[vector.Length];

		if (vector.Is1D())
		{
			BuildRow(values, indices, offset: 0, count: values.Length, order);
			return new VectorInt(vector.Gpu, indices, vector.Columns, cache: syncToGpu);
		}

		int cols = vector.ElementsPerRow();
		SortCpuParallelRows.ForEachRow(vector.RowCount(), cols, row =>
			BuildRow(values, indices, row * cols, cols, order));
		return new VectorInt(vector.Gpu, indices, vector.Columns, cache: syncToGpu);
	}

	static void BuildArgsortInto<T>(VectorBase<T> vector, VectorInt indices, T[] values, SortOrder order, bool syncToGpu)
		where T : unmanaged, IComparable<T>
	{
		ValidateArgsortShape(vector, indices);

		if (vector.Length == 0)
			return;

		// ArgsortIP always fully overwrites every element, so a stale/empty CPU-side buffer (e.g. a
		// freshly GPU-allocated `indices`) is fine — presize it here rather than relying on
		// CpuScope's sync-on-enter, which never pulls once residence flips to CPU-authoritative.
		if (indices.Value.Length != indices.Length)
			indices.Value = new int[indices.Length];

		using (indices.CpuScope(syncToGpu))
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

	static void ValidateArgsortShape<T>(VectorBase<T> vector, VectorInt indices) where T : unmanaged
	{
		if (!vector.Shape().MatchesDimensions(indices.Shape()))
			throw new ShapeMismatchException("ArgsortIP", vector.Shape(), indices.Shape());
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
