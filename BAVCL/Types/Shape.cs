using BAVCL.Exceptions;

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
			throw new ShapeMismatchException("FromStorage", $"length={length}, columns={columns}", "(invalid storage layout)");

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

	public Shape BroadcastWith(Shape other) =>
		new(BroadcastDim(Rows, other.Rows), BroadcastDim(Cols, other.Cols));

	static int BroadcastDim(int a, int b)
	{
		if (a == b)
			return a;

		if (a == 1)
			return b;

		if (b == 1)
			return a;

		throw new ShapeMismatchException("broadcast", $"({a})", $"({b})");
	}
}
