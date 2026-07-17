using System;

namespace BAVCL.Core.Exceptions;

/// <summary>
/// Thrown when operand shapes are incompatible with the requested operation.
/// </summary>
public class ShapeMismatchException(string operation, string shapeA, string shapeB) : Exception(
	$"Cannot perform {operation}: incompatible shapes {shapeA} and {shapeB}.");
