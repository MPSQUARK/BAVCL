using System;

namespace BAVCL.Core.Exceptions;

public class UnsupportedOperationException(string operationName) : Exception(
	$"Unsupported operation {operationName}.");
