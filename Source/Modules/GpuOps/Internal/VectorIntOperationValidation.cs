using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;

namespace BAVCL.Modules.GpuOps;

internal static class VectorIntOperationValidation
{
	static readonly Operations[] FloatOnlyOperations =
	[
		Operations.distance,
		Operations.magnitude,
		Operations.pow,
		Operations.flipPow,
	];

	static readonly Operations[] SupportedElementWiseOperations =
	[
		Operations.multiply,
		Operations.add,
		Operations.subtract,
		Operations.flipSubtract,
		Operations.divide,
		Operations.flipDivide,
		Operations.differenceSquared,
		Operations.modulo,
		Operations.flipModulo,
		Operations.leftShift,
		Operations.rightShift,
		Operations.bitwiseXor,
		Operations.bitwiseAnd,
	];

	static readonly Operations[] SupportedReduceOperations =
	[
		Operations.multiply,
		Operations.add,
		Operations.subtract,
		Operations.flipSubtract,
		Operations.divide,
		Operations.flipDivide,
	];

	internal static void ValidateOperation(Operations operation)
	{
		if (IsFloatOnly(operation))
			throw new InvalidOperationOnTypeException(operation, typeof(VectorInt));

		if (IsSupportedElementWise(operation))
			return;

		throw new UnsupportedOperationException(operation.ToString());
	}

	internal static void ValidateReduceOperation(Operations operation)
	{
		if (IsFloatOnly(operation))
			throw new InvalidOperationOnTypeException(operation, typeof(VectorInt));

		if (IsSupportedReduce(operation))
			return;

		throw new UnsupportedOperationException(operation.ToString());
	}

	static bool IsFloatOnly(Operations operation)
	{
		foreach (Operations floatOnly in FloatOnlyOperations)
		{
			if (floatOnly == operation)
				return true;
		}

		return false;
	}

	static bool IsSupportedElementWise(Operations operation)
	{
		foreach (Operations supported in SupportedElementWiseOperations)
		{
			if (supported == operation)
				return true;
		}

		return false;
	}

	static bool IsSupportedReduce(Operations operation)
	{
		foreach (Operations supported in SupportedReduceOperations)
		{
			if (supported == operation)
				return true;
		}

		return false;
	}
}
