using System;

namespace BAVCL.Core.Exceptions;

public class ShapeMismatchException(string operation, string shapeA, string shapeB) : Exception(
	$"Cannot perform {operation}: incompatible shapes {shapeA} and {shapeB}.");
