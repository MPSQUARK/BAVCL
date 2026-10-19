using System;

namespace BAVCL.Core.Exceptions;

/// <summary>Operand storage is not a 1D row vector (<see cref="VectorBase{T}.Is1DRowVector"/>).</summary>
public class RowVectorLayoutException(Shape shape) : Exception(
	$"Operation requires a 1D row coefficient vector (Columns=0). Got shape {shape}.")
{
}
