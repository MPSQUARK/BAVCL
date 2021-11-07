using System;

namespace DataScience.Ext
{
    public static partial class Extensions
    {
        public static float Max(this float[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            float max = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }

        public static float Max(this float[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Max(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            float max = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (float.IsInfinity(arr[i])) { continue; }
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }

    }

}
