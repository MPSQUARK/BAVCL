using System;

namespace BAVCL.Core.Exceptions;

public class InvalidOperationOnTypeException(Operations operation, Type type) : Exception(
	$"Invalid operation {operation} on type {type.Name}.");
