using System.Collections.Generic;
using System.Numerics;

namespace BAVCL.Ext;

public static class Generators
{
	public static IEnumerable<T> Arange<T>(T startValue, T endValue, T interval) where T : INumber<T>
	{
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

	public static IEnumerable<T> Linspace<T>(T startValue, T endValue, int num) where T : INumber<T>
	{
		T interval = (endValue - startValue) / T.CreateChecked(num);
		return Arange(startValue, endValue, interval);
	}

}
