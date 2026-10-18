using System;

namespace BAVCL.Core.Exceptions;

/// <summary>Operand storage is not a 2D matrix (<see cref="VectorBase{T}.Is2D"/>).</summary>
public class MatrixLayoutException(Shape shape) : Exception(
	$"Operation requires a 2D matrix (Columns > 1). Got shape {shape}.")
{
}
