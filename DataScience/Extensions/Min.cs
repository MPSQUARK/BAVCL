using System;

namespace DataScience.Ext
{
    public static partial class Extensions
    {
        public static float Min(this float[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            float min = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }

        public static float Min(this float[] arr, bool ignoreInf=true)
        {
            if (!ignoreInf) { return arr.Min(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            float min = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (float.IsInfinity(arr[i])) { continue; }
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }


    }

}
