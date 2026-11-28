using System;
using BAVCL.Modules.Structural;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core;

public abstract class VectorBase<T> : CacheableBase<T>, IIO where T : unmanaged
{
	protected internal volatile int _columns = 0;

	public virtual int Columns
	{
		get => _columns;
		set
		{
			Guard.ValidColumns(value);
			_columns = value;
		}
	}

	/// <summary>Creates an empty shell with no backing data or GPU buffer.</summary>
	protected VectorBase(GPU? gpu = null) : base(gpu) { }

	protected VectorBase(T[] values, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
		: base(values, upload, gpu) =>
		Columns = columns;

	protected VectorBase(int length, int columns = 0, Init init = Init.UnInit, GPU? gpu = null)
		: base(length, init, gpu) =>
		Columns = columns;

	/// <summary>Replaces storage with host <paramref name="values"/>, freeing any prior owned GPU buffer first.</summary>
	public virtual void Assign(T[] values, int columns = 0, Upload upload = Upload.Now)
	{
		ArgumentNullException.ThrowIfNull(values);
		Guard.ValidColumns(columns);
		AssignDataCore(values, upload);
		Columns = columns;
	}

	/// <summary>Replaces storage with a fresh GPU buffer of <paramref name="length"/> elements, freeing any prior owned GPU buffer first.</summary>
	public virtual void Assign(int length, int columns = 0, Init init = Init.UnInit)
	{
		Guard.ArgumentIsNonNegative(length);
		Guard.ValidColumns(columns);
		AssignLengthCore(length, init);
		Columns = columns;
	}

	/// <summary>Replaces storage with a device-to-device copy of <paramref name="deviceView"/>, freeing any prior owned GPU buffer first.</summary>
	public virtual void Assign(ArrayView1D<T, Stride1D.Dense> deviceView, int columns = 0)
	{
		Guard.ArgumentIsPositive(deviceView.IntLength);
		Guard.ValidColumns(columns);
		AssignViewCore(deviceView);
		Columns = columns;
	}

	public virtual void Print() => Console.WriteLine(ToString());

	public int RowCount()
	{
		if (Columns == 0)
			return 1;

		if (Columns == 1)
			return Length;

		return Length / Columns;
	}

	/// <summary>
	/// Number of elements per row. For 1D (<see cref="Is1D"/>) vectors this is the full length.
	/// </summary>
	public int ElementsPerRow() => Is1D() ? Length : Columns;

	public virtual BAVCL.Shape Shape() => BAVCL.Shape.FromStorage(Length, Columns);

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

	/// <summary>
	/// True for flat/single-row (<c>Columns == 0</c>) or column-vector (<c>Columns == 1</c>) storage —
	/// both lay out as one global, non-row-segmented sequence.
	/// </summary>
	public bool Is1D() => Columns <= 1;

	/// <summary>True for flat/single-row storage (<c>Columns == 0</c>), as distinct from column-vector storage.</summary>
	public bool Is1DRowVector() => Columns == 0;

	/// <summary>True for a true 2D matrix (<c>Columns > 1</c>), which sorts/reshapes row-wise.</summary>
	public bool Is2D() => Columns > 1;

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
		using (var scope = this.CpuScope(Upload.Defer))
		{
			EditableView<T> view = scope.View;
			view[index] = val;
		}
	}

	public void SetAt(int row, int col, T val)
	{
		int computedIndex = GetIndexFromCoordinatesForView(row, col);
		ValidateIndexForView(computedIndex);
		using (var scope = this.CpuScope(Upload.Defer))
		{
			EditableView<T> view = scope.View;
			view[computedIndex] = val;
		}
	}
}
