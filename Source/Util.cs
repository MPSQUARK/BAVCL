using ILGPU.Algorithms;
using System;

namespace BAVCL.Utility
{
    public class Util
    {

        public static bool IsClose(float val1, float val2, float threshold = 1e-5f)
        {
            return XMath.Abs(val1 - val2) < threshold ? true : false;
        }

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

        public static (int, int, bool) MinMaxInf(float[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            float max = arr[0];
            float min = arr[0];
            int i = 1;

            for (; i < arr.Length; i++)
            {
                if (float.IsInfinity(arr[i]) || float.IsNaN(arr[i]))
                {
                    if (max < 999) { max = 999; }
                    if (min > 999) { min = 999; }
                    break;
                }
                if (max < arr[i])
                {
                    max = arr[i];
                    continue;
                }
                if (min > arr[i])
                {
                    min = arr[i];
                    continue;
                }
            }
            
            if (i == arr.Length) {return ((int)min, (int)max, false); }

            for (; i < arr.Length; i++)
            {
                if (float.IsInfinity(arr[i]) || float.IsNaN(arr[i]))
                {
                    if (max < 999) { max = 999; }
                    if (min > 999) { min = 999; }
                    continue;
                }
                if (max < arr[i])
                {
                    max = arr[i];
                }
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return ((int)min, (int)max, true);
        }


    }



}
