namespace BAVCL;

/// <summary>
/// Element strides that map a broadcast output coordinate onto an operand index.
/// A zero stride repeats that axis, which is exactly what broadcasting a length-one axis means,
/// so operand addressing stays a multiply-add with no shape tests inside a kernel.
/// </summary>
public readonly struct BroadcastStrides(int row, int column)
{
	public readonly int Row = row;
	public readonly int Column = column;

	public static BroadcastStrides For(Shape operand) =>
		new(operand.Rows == 1 ? 0 : operand.Cols, operand.Cols == 1 ? 0 : 1);

	public int IndexOf(int row, int column) => (row * Row) + (column * Column);
}
