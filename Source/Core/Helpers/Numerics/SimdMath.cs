using System;
using System.Numerics;
using SimdVector = System.Numerics.Vector;

namespace BAVCL.Core.Helpers.Numerics;

/// <summary>Shared <see cref="System.Numerics.Vector{T}"/> helpers for CPU reductions.</summary>
internal static class SimdMath
{
	internal static int FloatLanes => Vector<float>.Count;

	internal static float HorizontalSum(Vector<float> values)
	{
		float total = 0f;
		for (int lane = 0; lane < Vector<float>.Count; lane++)
			total += values[lane];
		return total;
	}

    internal static void WidenToDoubleHalves(Vector<float> input, out Vector<double> lower, out Vector<double> upper) 
		=> SimdVector.Widen(input, out lower, out upper);

    internal static Vector<float> ConvertInt32ToFloat(Vector<int> values, Span<float> buffer)
	{
		for (int lane = 0; lane < Vector<int>.Count; lane++)
			buffer[lane] = values[lane];
		return new Vector<float>(buffer);
	}
}
