using System;

namespace BAVCL.Core.Exceptions;

/// <summary>Thrown when a mask's logical element count does not divide evenly into rows of the requested column count.</summary>
public sealed class MaskLayoutException(int elementCount, int columns)
	: Exception($"Mask storage layout is not rectangular: elementCount={elementCount}, columns={columns}.")
{
	public int ElementCount { get; } = elementCount;

	public int Columns { get; } = columns;
}
