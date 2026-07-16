using System;

namespace BAVCL.Core.Exceptions;

public class LengthMismatchException(string operation, int lengthA, int lengthB) : Exception(
	$"Cannot perform {operation}: operand lengths must match ({lengthA} != {lengthB}).");
