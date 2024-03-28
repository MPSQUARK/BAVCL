using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    // public static Vec<T> Arange(GPU gpu, T startval, T endval, T interval, int Columns = 1, bool cache = true)
    // {
    // 	int steps = (int)((endval - startval) / interval);
    // 	if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }
    // 	if (endval % interval != 0) { steps++; }

    // 	float[] values = new float[steps];

    // 	for (int i = 0; i < steps; i++)
    // 		values[i] = startval + (i * interval);


    // 	return new Vec<T>(gpu, values, Columns, cache);
    // }


    // public static float[] Arange(float startval, float endval, float interval)
    // {
    // 	int steps = (int)((endval - startval) / interval);
    // 	if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }
    // 	if (endval % interval != 0) { steps++; }

    // 	float[] values = new float[steps];

    // 	for (int i = 0; i < steps; i++)
    // 		values[i] = startval + (i * interval);

    // 	return values;
    // }
}
