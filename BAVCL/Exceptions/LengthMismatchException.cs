using System;

namespace BAVCL.Exceptions;

/// <summary>
/// Thrown when operands require equal lengths but the lengths differ.
/// </summary>
public class LengthMismatchException(string operation, int lengthA, int lengthB) : Exception(
	$"Cannot perform {operation}: operand lengths must match ({lengthA} != {lengthB}).");
