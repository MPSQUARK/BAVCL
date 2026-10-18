using System;
using BAVCL.Core.Exceptions;

namespace BAVCL;

public readonly record struct Shape(int Rows, int Cols)
{
	public bool MatchesDimensions(Shape other) => Rows == other.Rows && Cols == other.Cols;

	public int ElementCount => Rows * Cols;

	public override string ToString() => $"({Rows},{Cols})";

	public static Shape FromStorage(int length, int columns)
	{
		if (columns == 0)
			return new Shape(1, length);

		if (columns == 1)
			return new Shape(length, 1);

		if (columns <= 0 || length % columns != 0)
			throw new ArgumentException($"Invalid storage layout: length={length}, columns={columns}.", nameof(columns));

		return new Shape(length / columns, columns);
	}

	public int ToStorageColumns()
	{
		if (Cols == 1)
			return 1;

		if (Rows == 1)
			return 0;

		return Cols;
	}

	public Shape BroadcastWith(Shape other)
	{
		if (!TryBroadcastDim(Rows, other.Rows, out int rows)
			|| !TryBroadcastDim(Cols, other.Cols, out int cols))
			throw new ShapeMismatchException(this, other);

		return new Shape(rows, cols);
	}

	static bool TryBroadcastDim(int a, int b, out int result)
	{
		if (a == b)
		{
			result = a;
			return true;
		}

		if (a == 1)
		{
			result = b;
			return true;
		}

		if (b == 1)
		{
			result = a;
			return true;
		}

		result = 0;
		return false;
	}
}
