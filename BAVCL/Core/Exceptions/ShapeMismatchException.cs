using System;

namespace BAVCL.Core.Exceptions;

public class ShapeMismatchException : Exception
{
	public ShapeMismatchException(string operation, string shapeA, string shapeB)
		: base($"Cannot perform {operation}: incompatible shapes {shapeA} and {shapeB}.") { }

	public ShapeMismatchException(string operation, Shape shapeA, Shape shapeB)
		: this(operation, shapeA.ToString(), shapeB.ToString()) { }
}
