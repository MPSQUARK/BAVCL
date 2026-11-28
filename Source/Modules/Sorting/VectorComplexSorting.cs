using System;
using System.Collections.Generic;
using System.Numerics;

namespace BAVCL.Modules.Sorting;

/// <summary>
/// Sorting and argsorting for complex vectors.
/// <para>
/// Sorting uses only the selected key. Equal keys have unspecified relative order;
/// <see cref="ComplexSortMode.Complex"/> uses real as its primary component and
/// imaginary as its secondary component.
/// </para>
/// </summary>
public static class VectorComplexSorting
{
	extension(VectorComplex64 vector)
	{
		public VectorComplex64 Sort(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			order switch
			{
				SortOrder.Ascending => vector.SortAsc(mode),
				SortOrder.Descending => vector.SortDesc(mode),
				_ => throw new ArgumentOutOfRangeException(nameof(order)),
			};

		public void SortIP(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending)
		{
			switch (order)
			{
				case SortOrder.Ascending:
					vector.SortAscIP(mode);
					return;
				case SortOrder.Descending:
					vector.SortDescIP(mode);
					return;
				default:
					throw new ArgumentOutOfRangeException(nameof(order));
			}
		}

		public VectorComplex64 SortAsc(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			SortCore(vector, mode, SortOrder.Ascending);

		public VectorComplex64 SortDesc(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			SortCore(vector, mode, SortOrder.Descending);

		public void SortAscIP(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			SortInPlace(vector, mode, SortOrder.Ascending);

		public void SortDescIP(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			SortInPlace(vector, mode, SortOrder.Descending);

		public VectorInt Argsort(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			order switch
			{
				SortOrder.Ascending => vector.ArgsortAsc(mode),
				SortOrder.Descending => vector.ArgsortDesc(mode),
				_ => throw new ArgumentOutOfRangeException(nameof(order)),
			};

		public VectorInt ArgsortAsc(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ArgsortCore(vector, mode, SortOrder.Ascending);

		public VectorInt ArgsortDesc(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ArgsortCore(vector, mode, SortOrder.Descending);

		public void ArgsortAscIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ArgsortCoreIP(vector, indices, mode, SortOrder.Ascending);

		public void ArgsortDescIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ArgsortCoreIP(vector, indices, mode, SortOrder.Descending);

		public VectorComplex64 SortX(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			order switch
			{
				SortOrder.Ascending => vector.SortAscX(mode),
				SortOrder.Descending => vector.SortDescX(mode),
				_ => throw new ArgumentOutOfRangeException(nameof(order)),
			};

		public void SortXIP(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending)
		{
			switch (order)
			{
				case SortOrder.Ascending:
					vector.SortAscXIP(mode);
					return;
				case SortOrder.Descending:
					vector.SortDescXIP(mode);
					return;
				default:
					throw new ArgumentOutOfRangeException(nameof(order));
			}
		}

		public VectorInt ArgsortX(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			order switch
			{
				SortOrder.Ascending => vector.ArgsortAscX(mode),
				SortOrder.Descending => vector.ArgsortDescX(mode),
				_ => throw new ArgumentOutOfRangeException(nameof(order)),
			};

		public void ArgsortXIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending)
		{
			switch (order)
			{
				case SortOrder.Ascending:
					vector.ArgsortAscXIP(indices, mode);
					return;
				case SortOrder.Descending:
					vector.ArgsortDescXIP(indices, mode);
					return;
				default:
					throw new ArgumentOutOfRangeException(nameof(order));
			}
		}

		public VectorComplex64 SortAscX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortAscX(vector, mode);

		public VectorComplex64 SortDescX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortDescX(vector, mode);

		public void SortAscXIP(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortAscXIP(vector, mode);

		public void SortDescXIP(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortDescXIP(vector, mode);

		public VectorInt ArgsortAscX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortAscX(vector, mode);

		public VectorInt ArgsortDescX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortDescX(vector, mode);

		public void ArgsortAscXIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortAscXIP(vector, indices, mode);

		public void ArgsortDescXIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortDescXIP(vector, indices, mode);
	}

	static VectorComplex64 SortCore(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		Complex[] values = vector.RetrieveReadOnlySpan().ToArray();
		SortRows(values, vector.Columns, mode, order);
		return new VectorComplex64(vector.Gpu, values, vector.Columns);
	}

	static void SortInPlace(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		using (var scope = vector.CpuScope())
		{
			Complex[] values = vector.Value;
			SortRows(values, vector.Columns, mode, order);
			EditableView<Complex> view = scope.View;
			for (int i = 0; i < values.Length; i++)
				view[i] = values[i];
		}
	}

	static VectorInt ArgsortCore(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		Complex[] values = vector.RetrieveReadOnlySpan().ToArray();
		int[] indices = new int[values.Length];
		for (int row = 0; row < RowCount(vector.Length, vector.Columns); row++)
		{
			int offset = vector.Is1D() ? 0 : row * vector.Columns;
			int count = vector.Is1D() ? values.Length : vector.Columns;
			SortIndexFill.WriteRowLocal(indices, offset, count);
			Array.Sort(indices, offset, count, Comparer<int>.Create((left, right) =>
				Compare(values[offset + left], values[offset + right], mode, order)));
		}
		return new VectorInt(vector.Gpu, indices, vector.Columns);
	}

	static void ArgsortCoreIP(VectorComplex64 vector, VectorInt indices, ComplexSortMode mode, SortOrder order)
	{
		Guard.MatchingDimensions(vector.Shape(), indices.Shape());
		VectorInt result = ArgsortCore(vector, mode, order);
		using var scope = indices.CpuScope();
		EditableView<int> view = scope.View;
		ReadOnlySpan<int> source = result.RetrieveReadOnlySpan();
		for (int i = 0; i < source.Length; i++)
			view[i] = source[i];
	}

	static void SortRows(Complex[] values, int columns, ComplexSortMode mode, SortOrder order)
	{
		if (values.Length < 2)
			return;
		if (columns <= 1)
		{
			Array.Sort(values, Comparer<Complex>.Create((left, right) => Compare(left, right, mode, order)));
			return;
		}
		for (int row = 0; row < values.Length / columns; row++)
			Array.Sort(values, row * columns, columns, Comparer<Complex>.Create((left, right) => Compare(left, right, mode, order)));
	}

	static int Compare(Complex left, Complex right, ComplexSortMode mode, SortOrder order)
	{
		int result = mode switch
		{
			ComplexSortMode.Magnitude => CompareDouble(left.Magnitude, right.Magnitude),
			ComplexSortMode.Complex => CompareComponents(left, right),
			ComplexSortMode.Real => CompareDouble(left.Real, right.Real),
			ComplexSortMode.Imaginary => CompareDouble(left.Imaginary, right.Imaginary),
			_ => throw new ArgumentOutOfRangeException(nameof(mode)),
		};
		return order == SortOrder.Descending ? -result : result;
	}

	static int CompareComponents(Complex left, Complex right)
	{
		int real = CompareDouble(left.Real, right.Real);
		return real != 0 ? real : CompareDouble(left.Imaginary, right.Imaginary);
	}

	static int CompareDouble(double left, double right)
	{
		if (left < right)
			return -1;
		if (left > right)
			return 1;
		return GPU.SortableDouble(left).CompareTo(GPU.SortableDouble(right));
	}

	static int RowCount(int length, int columns) => columns <= 1 ? 1 : length / columns;

	static void ValidateMode(ComplexSortMode mode)
	{
		if (mode is not (ComplexSortMode.Magnitude or ComplexSortMode.Complex or
			ComplexSortMode.Real or ComplexSortMode.Imaginary))
			throw new ArgumentOutOfRangeException(nameof(mode));
	}
}
