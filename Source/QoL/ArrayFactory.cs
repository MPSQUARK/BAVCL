using System;

namespace BAVCL.QoL;

/// <summary>Host array constructors for any element type.</summary>
public static class ArrayFactory
{
	/// <summary>Returns a new array of <paramref name="length"/> elements, all set to <paramref name="value"/>.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
	public static T[] FilledArray<T>(int length, T value)
	{
		Guard.ArgumentIsNonNegative(length);
		T[] values = new T[length];
		Array.Fill(values, value);
		return values;
	}
}
