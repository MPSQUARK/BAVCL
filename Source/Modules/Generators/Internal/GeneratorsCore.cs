using System;

namespace BAVCL.Modules.Generators;

internal static class GeneratorsCore
{
	internal static float[] Arange(float startval, float endval, float interval)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsFinite(interval);
		Guard.ArgumentIsNotZero(interval);
		if (endval < startval && interval > 0) interval = -interval;

		int steps = CountSamples(endval - startval, interval);
		float[] values = new float[steps];

		for (int i = 0; i < steps; i++)
			values[i] = startval + (i * interval);

		return values;
	}

	internal static double[] Arange(double startval, double endval, double interval)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsFinite(interval);
		Guard.ArgumentIsNotZero(interval);
		if (endval < startval && interval > 0) interval = -interval;

		int steps = CountSamples(endval - startval, interval);
		double[] values = new double[steps];

		for (int i = 0; i < steps; i++)
			values[i] = startval + (i * interval);

		return values;
	}

	internal static int[] Arange(int startval, int endval, int interval)
	{
		Guard.ArgumentIsNotZero(interval);
		if (endval < startval && interval > 0) interval = -interval;

		int steps = CountSamples(startval, endval, interval);
		int[] values = new int[steps];

		for (int i = 0; i < steps; i++)
			values[i] = startval + (i * interval);

		return values;
	}

	internal static float[] Linspace(float startval, float endval, int steps)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsNonNegative(steps);
		float interval = steps > 1 ? (endval - startval) / (steps - 1) : 0f;

		float[] arr = new float[steps];

		for (int i = 0; i < steps; i++)
			arr[i] = startval + (i * interval);

		if (steps > 1)
			arr[steps - 1] = endval;

		return arr;
	}

	internal static double[] Linspace(double startval, double endval, int steps)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsNonNegative(steps);
		double interval = steps > 1 ? (endval - startval) / (steps - 1) : 0d;

		double[] arr = new double[steps];

		for (int i = 0; i < steps; i++)
			arr[i] = startval + (i * interval);

		if (steps > 1)
			arr[steps - 1] = endval;

		return arr;
	}

	internal static int[] Linspace(int startval, int endval, int steps)
	{
		Guard.ArgumentIsNonNegative(steps);
		double interval = steps > 1 ? ((double)endval - startval) / (steps - 1) : 0d;

		int[] arr = new int[steps];

		for (int i = 0; i < steps; i++)
			arr[i] = (int)(startval + (i * interval));

		if (steps > 1)
			arr[steps - 1] = endval;

		return arr;
	}

	/// <summary><c>ceil(span / step)</c> in float. A non-positive count is an empty range.</summary>
	internal static int CountSamples(float span, float step)
	{
		float count = MathF.Ceiling(span / step);
		if (!(count > 0f))
			return 0;

		// int.MaxValue is not representable as a float, so this rejects 2^31 and above.
		if (count >= int.MaxValue)
			throw new ArgumentOutOfRangeException(null, "The range produces more samples than an array can hold.");

		return (int)count;
	}

	/// <summary><c>ceil(span / step)</c> in double. A non-positive count is an empty range.</summary>
	internal static int CountSamples(double span, double step)
	{
		double count = Math.Ceiling(span / step);
		if (!(count > 0d))
			return 0;
		if (count > int.MaxValue)
			throw new ArgumentOutOfRangeException(null, "The range produces more samples than an array can hold.");

		return (int)count;
	}

	/// <summary>
	/// Sample count from <paramref name="startval"/> toward <paramref name="endval"/> by <paramref name="step"/>.
	/// The distance between two ints does not always fit in an int, so it is counted in a 32-bit unsigned value.
	/// </summary>
	internal static int CountSamples(int startval, int endval, int step)
	{
		if (endval == startval || (endval > startval) != (step > 0))
			return 0;

		uint distance = endval > startval
			? (uint)endval - (uint)startval
			: (uint)startval - (uint)endval;
		uint stride = step == int.MinValue ? 1u << 31 : (uint)Math.Abs(step);
		uint count = distance / stride;
		if (distance % stride != 0)
			count++;
		if (count > (uint)int.MaxValue)
			throw new ArgumentOutOfRangeException(null, "The range produces more samples than an array can hold.");

		return (int)count;
	}
}
