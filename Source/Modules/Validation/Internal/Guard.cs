using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BAVCL.Core.Exceptions;
using BAVCL.Types;

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

	internal static void DivisorIsNotZero(int value, [CallerArgumentExpression(nameof(value))] string? name = null)
	{
		if (value == 0)
			throw new DivideByZeroException($"Cannot divide by zero ({name ?? "value"}).");
	}

	internal static void ArgumentIsNotZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? name = null)
		where T : INumberBase<T>
	{
		if (T.IsZero(value))
			throw new ArgumentOutOfRangeException(name ?? nameof(value), value, "Value must be non-zero.");
	}

	/// <summary>Throws unless <paramref name="value"/> is greater than zero.</summary>
	internal static void ArgumentIsPositive<T>(T value, [CallerArgumentExpression(nameof(value))] string? name = null)
		where T : INumber<T>
	{
		if (value <= T.Zero)
			throw new ArgumentOutOfRangeException(name ?? nameof(value), value, "Value must be positive.");
	}

	/// <summary>
	/// Throws when a range yields no samples. Vectors must hold at least one element, so a factory cannot
	/// return an empty range.
	/// </summary>
	internal static void RangeIsNotEmpty(int sampleCount)
	{
		if (sampleCount == 0)
			throw new ArgumentException("The range produces no samples. Vectors must hold at least one element.");
	}

	/// <summary>
	/// Throws unless <paramref name="value"/> is non-negative. The check is written as <c>!(value &gt;= 0)</c>
	/// rather than <c>value &lt; 0</c> so that NaN, for which every comparison is false, is rejected too.
	/// </summary>
	internal static void ArgumentIsNonNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? name = null)
		where T : INumber<T>
	{
		if (!(value >= T.Zero))
			throw new ArgumentOutOfRangeException(name ?? nameof(value), value, "Value must be non-negative.");
	}

	/// <summary>
	/// Throws unless <paramref name="value"/> is finite, so NaN and infinities are rejected.
	/// </summary>
	internal static void ArgumentIsFinite<T>(T value, [CallerArgumentExpression(nameof(value))] string? name = null)
		where T : INumberBase<T>
	{
		if (!T.IsFinite(value))
			throw new ArgumentOutOfRangeException(name ?? nameof(value), value, "Value must be finite.");
	}

	/// <summary>
	/// Throws unless <paramref name="value"/> is less than <paramref name="upperBound"/>. The check is written
	/// as <c>!(value &lt; upperBound)</c> so that NaN, for which every comparison is false, is rejected too.
	/// </summary>
	internal static void ArgumentIsLessThan<T>(
		T value,
		T upperBound,
		[CallerArgumentExpression(nameof(value))] string? name = null,
		[CallerArgumentExpression(nameof(upperBound))] string? upperName = null)
		where T : INumber<T>
	{
		if (!(value < upperBound))
			throw new ArgumentOutOfRangeException(
				name ?? nameof(value), value, $"{name} must be less than {upperName}.");
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

	internal static void MatchingDimensions(Shape left, Shape right)
	{
		if (left.MatchesDimensions(right))
			return;

		throw new ShapeMismatchException(left, right);
	}

	internal static void IsRowVector1D<T>(VectorBase<T> vector)
		where T : unmanaged
	{
		if (vector.Is1DRowVector())
			return;

		throw new RowVectorLayoutException(vector.Shape());
	}

	internal static void IsVector2D<T>(VectorBase<T> vector)
		where T : unmanaged
	{
		if (vector.Is2D())
			return;

		throw new MatrixLayoutException(vector.Shape());
	}

	internal static void MatchingGpu(GPU left, GPU right) =>
		GpuValidation.IsSameGpu(left, right);

	internal static void MatchingGpu<T>(VectorBase<T> left, VectorBase<T> right)
		where T : unmanaged =>
		MatchingGpu(left.Gpu, right.Gpu);

	internal static void MatchingGpu<T>(VectorBase<T> vector, Mask mask)
		where T : unmanaged =>
		MatchingGpu(vector.Gpu, mask.Gpu);

	internal static void RollingWindow(int window, int length, [CallerArgumentExpression(nameof(window))] string? windowName = null)
	{
		if (window <= 0)
			throw new ArgumentOutOfRangeException(windowName ?? nameof(window), window, "Window size must be positive.");

		if (length == 0)
			return;
	}

	internal static void MatchingGpuPairIndices<T>(VectorBase<T> values, VectorInt indices)
		where T : unmanaged
	{
		MatchingGpu(values.Gpu, indices.Gpu);
		MatchingDimensions(values.Shape(), indices.Shape());
	}

	internal static void ValidColumns(int columns, [CallerArgumentExpression(nameof(columns))] string? name = null)
	{
		if (columns < 0)
			throw new ArgumentOutOfRangeException(name ?? nameof(columns), columns, $"Columns must be zero or greater. Received {columns}.");
	}

	internal static void NotEmpty(ICacheable cacheable)
	{
		if (cacheable.Residence == Residence.None)
			throw new InvalidOperationException(
				$"Cannot operate on an empty {cacheable.GetType().Name}. Call Assign or Possess first.");
	}

	internal static void IsEmptyForPossession(ICacheable cacheable)
	{
		if (cacheable.Residence != Residence.None)
			throw new InvalidOperationException(
				$"{cacheable.GetType().Name} must be empty before it can be possessed by a pooled buffer.");
	}

	internal static void AcquirePossession<T>(CacheableBase<T> target) where T : unmanaged
	{
		if (!target.TryAcquirePossession())
			throw new InvalidOperationException(
				$"{target.GetType().Name} is already possessed by another pooled buffer.");
	}

	internal static void CanReassign<T>(CacheableBase<T> cacheable) where T : unmanaged
	{
		if (cacheable.IsPossessed)
			throw new InvalidOperationException(
				$"Cannot reassign {cacheable.GetType().Name} while it is possessed by a pooled buffer.");

		if (cacheable.LiveCount > 0 || ResidenceHelper.IsActiveCpu(cacheable.Residence) || ResidenceHelper.IsActiveGpu(cacheable.Residence))
			throw new InvalidOperationException(
				$"Cannot reassign {cacheable.GetType().Name} while a CPU or GPU scope is active.");
	}

	internal static void BufferReleased(uint id, uint remaining)
	{
		if (remaining != 0)
			throw new InvalidOperationException(
				$"Cannot release GPU buffer {id} while it is still live.");
	}
}
