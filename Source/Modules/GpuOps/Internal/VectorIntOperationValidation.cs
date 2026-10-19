using System.Collections.Generic;
using BAVCL.Core.Exceptions;

namespace BAVCL.Modules.GpuOps;

internal static class VectorIntOperationValidation
{
	static readonly HashSet<Operations> FloatOnlyOperations =
	[
		Operations.distance,
		Operations.magnitude,
		Operations.pow,
		Operations.flipPow,
	];

	static readonly HashSet<Operations> SupportedElementWiseOperations =
	[
		Operations.multiply,
		Operations.add,
		Operations.subtract,
		Operations.divide,
		Operations.flipDivide,
		Operations.flipSubtract,
		Operations.differenceSquared,
		Operations.modulo,
		Operations.flipModulo,
		Operations.leftShift,
		Operations.rightShift,
		Operations.bitwiseXor,
		Operations.bitwiseAnd,
	];

	static readonly HashSet<Operations> SupportedReduceOperations =
	[
		Operations.multiply,
		Operations.add,
		Operations.subtract,
		Operations.divide,
		Operations.flipDivide,
		Operations.flipSubtract,
	];

	internal static void ValidateOperation(Operations operation)
	{
		if (FloatOnlyOperations.Contains(operation))
			throw new InvalidOperationOnTypeException(operation, typeof(VectorInt));

		if (SupportedElementWiseOperations.Contains(operation))
			return;

		throw new UnsupportedOperationException(operation.ToString());
	}

	internal static void ValidateReduceOperation(Operations operation)
	{
		if (FloatOnlyOperations.Contains(operation))
			throw new InvalidOperationOnTypeException(operation, typeof(VectorInt));

		if (SupportedReduceOperations.Contains(operation))
			return;

		throw new UnsupportedOperationException(operation.ToString());
	}
}
