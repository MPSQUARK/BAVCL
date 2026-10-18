using System;

namespace BAVCL.Core.Exceptions;

public class ShapeMismatchException : Exception
{
	public string Operation { get; }

	public Shape? ShapeA { get; }

	public Shape? ShapeB { get; }

	public ShapeMismatchException(Shape shapeA, Shape shapeB)
		: base(FormatIncompatibleShapes(shapeA, shapeB))
	{
		Operation = string.Empty;
		ShapeA = shapeA;
		ShapeB = shapeB;
	}

	public ShapeMismatchException(string operation, Shape shapeA, Shape shapeB)
		: base(FormatMessage(operation, shapeA, shapeB))
	{
		Operation = operation;
		ShapeA = shapeA;
		ShapeB = shapeB;
	}

	/// <summary>When one or both sides are not representable as <see cref="Shape"/> (storage layout, element counts).</summary>
	public ShapeMismatchException(string operation, string shapeADetail, string shapeBDetail)
		: base(FormatMessage(operation, shapeADetail, shapeBDetail))
	{
		Operation = operation;
	}

	static string FormatIncompatibleShapes(Shape shapeA, Shape shapeB) =>
		$"Incompatible shapes {shapeA} and {shapeB}.";

	static string FormatMessage(string operation, Shape shapeA, Shape shapeB) =>
		FormatMessage(operation, shapeA.ToString(), shapeB.ToString());

	static string FormatMessage(string operation, string shapeA, string shapeB) =>
		$"Cannot perform {operation}: incompatible shapes {shapeA} and {shapeB}.";
}
