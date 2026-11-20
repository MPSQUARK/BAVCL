using System;

namespace BAVCL.QoL;

/// <summary>Host array constructors for any element type.</summary>
public static class ArrayFactory
{
	/// <summary>Returns a new array of <paramref name="length"/> elements, all set to <c>default(T)</c>.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
	public static T[] ZeroArray<T>(int length)
	{
		Guard.ArgumentIsNonNegative(length);
		return new T[length];
	}

	/// <summary>Returns a new array of <paramref name="length"/> elements, all set to <paramref name="value"/>.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
	public static T[] FilledArray<T>(int length, T value)
	{
		T[] values = ZeroArray<T>(length);
		Array.Fill(values, value);
		return values;
	}
}
