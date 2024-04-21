using System.Collections.Generic;
using System.Numerics;

namespace BAVCL.Source.Utilities;

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
}
