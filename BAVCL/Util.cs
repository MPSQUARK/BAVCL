using ILGPU.Algorithms;
using System;
using System.Linq;
using System.Text;

namespace BAVCL.Utility;

public class Util
{

    public static bool IsClose(float val1, float val2, float threshold = 1e-5f) => XMath.Abs(val1 - val2) <= threshold;

    public static float Max(float[] arr, bool NonInf = true)
    {
        if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

        float max = arr[0];

        if (NonInf)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (float.IsInfinity(arr[i]) || float.IsNaN(arr[i]))
                {
                    if (max < 999) { max = 999; }
                    continue;
                }
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }
            return max;
        }

        for (int i = 1; i < arr.Length; i++)
        {
            if (max < arr[i])
            {
                max = arr[i];
            }
        }

        return max;
    }

    public static float Min(float[] arr, bool NonInf = true)
    {
        if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

        float min = arr[0];

        if (NonInf)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (float.IsInfinity(arr[i]) || float.IsNaN(arr[i]))
                {
                    if (min > 999) { min = 999; }
                    continue;
                }
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }
            return min;
        }

        for (int i = 1; i < arr.Length; i++)
        {
            if (min > arr[i])
            {
                min = arr[i];
            }
        }

        return min;
    }

    public static (float, float, bool) MinMaxInf(float[] arr) => MinMaxInf(arr.AsSpan());

    public static (float, float, bool) MinMaxInf(ReadOnlySpan<float> span)
    {
        if (span.Length == 0) { throw new Exception("Cannot Be Length 0"); }

        float max = span[0];
        float min = span[0];
        int i = 1;

        for (; i < span.Length; i++)
        {
            if (float.IsInfinity(span[i]) || float.IsNaN(span[i]))
            {
                if (max < 999) { max = 999; }
                if (min > 999) { min = 999; }
                break;
            }
            if (max < span[i])
            {
                max = span[i];
                continue;
            }
            if (min > span[i])
            {
                min = span[i];
                continue;
            }
        }

        if (i == span.Length) { return (min, max, false); }

        for (; i < span.Length; i++)
        {
            if (float.IsInfinity(span[i]) || float.IsNaN(span[i]))
            {
                if (max < 999) { max = 999; }
                if (min > 999) { min = 999; }
                continue;
            }
            if (max < span[i])
            {
                max = span[i];
            }
            if (min > span[i])
            {
                min = span[i];
            }
        }

        return (min, max, true);
    }


}
