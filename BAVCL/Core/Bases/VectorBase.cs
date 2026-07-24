using System;
using BAVCL.Modules.Structural;

namespace BAVCL.Core;

public abstract class VectorBase<T> : CacheableBase<T>, IIO where T : unmanaged
{
	protected internal int _columns = 0;

	public virtual int Columns
	{
		get => _columns;
		set
		{
			if (value < 0)
				throw new Exception($"Columns must be zero or greater. Recieved {value}");

			_columns = value;
		}
	}

	protected VectorBase(GPU gpu, T[] value, int columns = 0, bool cache = true) : base(gpu, value, cache) =>
		Columns = columns;

	protected VectorBase(GPU gpu, int length, int columns = 0) : base(gpu, length) =>
		Columns = columns;

	public virtual void Print() => Console.WriteLine(ToString());

	public int RowCount()
	{
		if (Columns == 0)
			return 1;

		if (Columns == 1)
			return Length;

		return Length / Columns;
	}

	public virtual Core.Shape Shape() => Core.Shape.FromStorage(Length, Columns);

	internal void ValidateIndexForView(int index)
	{
		if (index < 0 || index >= Length)
			throw new IndexOutOfRangeException($"Index {index} is out of range for vector of length {Length}.");
	}

	internal int GetIndexFromCoordinatesForView(int row, int col)
	{
		int index = row * Columns + col;
		ValidateIndexForView(index);
		return index;
	}

	public string ToCSV() => FormattingCore.ToCsv(this);

	public bool IsRectangular() => Columns == 0 || Length % Columns == 0;

	public bool Is1D() => Columns == 0;

	public T this[int i]
	{
		get => GetAt(i);
		set => SetAt(i, value);
	}

	public T this[int row, int col]
	{
		get => GetAt(row, col);
		set => SetAt(row, col, value);
	}

	public T GetAt(int index)
	{
		ValidateIndexForView(index);
		return RetrieveReadOnlySpan()[index];
	}

	public T GetAt(int row, int col)
	{
		int computedIndex = GetIndexFromCoordinatesForView(row, col);
		return RetrieveReadOnlySpan()[computedIndex];
	}

	public void SetAt(int index, T val)
	{
		ValidateIndexForView(index);
		using (var scope = this.CpuScope())
		{
			EditableView<T> view = scope.View;
			view[index] = val;
		}
	}

	public void SetAt(int row, int col, T val)
	{
		int computedIndex = GetIndexFromCoordinatesForView(row, col);
		ValidateIndexForView(computedIndex);
		using (var scope = this.CpuScope())
		{
			EditableView<T> view = scope.View;
			view[computedIndex] = val;
		}
	}
}
