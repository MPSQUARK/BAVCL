using System;
using System.Runtime.CompilerServices;
using BAVCL.Core.Exceptions;

namespace BAVCL.Modules.Validation;

/// <summary>
/// Argument validation in the style of
/// <see href="https://learn.microsoft.com/dotnet/communitytoolkit/diagnostics/guard">Community Toolkit Guard</see>.
/// Domain math failures throw domain exceptions, not <see cref="ArgumentException"/>.
/// </summary>
internal static class Guard
{
	internal const float PercentileMin = 0f;
	internal const float PercentileMax = 100f;

	internal static void IsNotZero(int value, [CallerArgumentExpression(nameof(value))] string? name = null)
	{
		if (value == 0)
			throw new DivideByZeroException($"Cannot divide by zero ({name ?? "value"}).");
	}

	internal static void IsNotEmpty(int length, [CallerArgumentExpression(nameof(length))] string? name = null)
	{
		if (length == 0)
			throw new InvalidOperationException($"Sequence contains no elements ({name ?? "length"}).");
	}

	internal static void IsInRangeInclusive(
		float value,
		float min,
		float max,
		[CallerArgumentExpression(nameof(value))] string? name = null)
	{
		if (float.IsNaN(value) || value < min || value > max)
			throw new FixedRangeException(name!, value, min, max);
	}

	internal static void MatchingDimensions(Shape left, Shape right, string operation)
	{
		if (left.MatchesDimensions(right))
			return;

		throw new ShapeMismatchException(operation, left, right);
	}

	internal static void MatchingGpuPairIndices<T>(VectorBase<T> values, VectorInt indices, string operation)
		where T : unmanaged
	{
		GpuValidation.IsSameGpu(values.Gpu, indices.Gpu);
		MatchingDimensions(values.Shape(), indices.Shape(), operation);
	}
}
