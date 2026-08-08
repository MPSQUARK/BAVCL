using System;

namespace BAVCL.Core.Helpers;

public static class FunctionalExtensions
{
	/// <summary>Runs <paramref name="action"/> on <paramref name="value"/>, then returns <paramref name="value"/>.</summary>
	public static T Also<T>(this T value, Action<T> action)
	{
		action(value);
		return value;
	}
}
