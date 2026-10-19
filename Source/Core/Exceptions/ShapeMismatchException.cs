using System;

namespace BAVCL.Core.Exceptions;

public class ShapeMismatchException(Shape shapeA, Shape shapeB) : Exception($"Incompatible shapes {shapeA} and {shapeB}.")
{}
