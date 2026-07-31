using System;
using System.Collections.Generic;
using System.Numerics;
using ILGPU.Algorithms;

namespace BAVCL.Modules.Generators;

internal static class GeneratorsCore
{
	internal static IEnumerable<T> ArangeEnumerable<T>(T startValue, T endValue, T interval) where T : INumber<T>
	{
		if (interval == T.Zero)
			throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be non-zero.");

		if (endValue < startValue && interval > T.Zero)
			interval = -interval;

        if (interval > T.Zero)
		{
			for (T i = startValue; i < endValue; i += interval)
				yield return i;
		}
		else if (interval < T.Zero)
		{
			for (T i = startValue; i > endValue; i += interval)
				yield return i;
		}
	}

	internal static IEnumerable<T> LinspaceEnumerable<T>(T startValue, T endValue, int num) where T : INumber<T>
	{
		if (num <= 0)
			throw new ArgumentOutOfRangeException(nameof(num), "Number of steps must be positive.");

		if (num == 1)
		{
			yield return startValue;
			yield break;
		}

		T step = (endValue - startValue) / T.CreateChecked(num - 1);
		for (int i = 0; i < num; i++)
			yield return startValue + step * T.CreateChecked(i);
	}

	internal static T[] Arange<T>(T startValue, T endValue, T interval) where T : INumber<T> =>
		[.. ArangeEnumerable(startValue, endValue, interval)];

	internal static T[] Linspace<T>(T startValue, T endValue, int num) where T : INumber<T> =>
		[.. LinspaceEnumerable(startValue, endValue, num)];

	// Note: Above Generic generators are for broad INumber support. Below are specific impl. for performance.

	internal static float[] Arange(float startval, float endval, float interval)
	{
		int steps = (int)((endval - startval) / interval);
		if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }
		if (endval % interval != 0) { steps++; }

		float[] values = new float[steps];

		for (int i = 0; i < steps; i++)
			values[i] = startval + (i * interval);

		return values;
	}

	internal static float[] Linspace(float startval, float endval, int steps)
	{
		if (steps <= 1) throw new Exception("Cannot make linspace with less than 1 steps");
		float interval = (endval - startval) / (steps - 1);

		float[] arr = new float[steps];

		for (int i = 0; i < steps; i++)
			arr[i] = startval + (i * interval);

		return arr;
	}
}
