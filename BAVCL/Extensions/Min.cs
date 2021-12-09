using System;

namespace BAVCL.Ext
{
    public static partial class Extensions
    {
        // FLOATS
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


        // DOUBLES
        public static double Min(this double[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            double min = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }

        public static double Min(this double[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Min(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            double min = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (double.IsInfinity(arr[i])) { continue; }
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }


        // INTS
        public static int Min(this int[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            int min = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }

        public static int Min(this int[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Min(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            int min = arr[0];

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


        // LONGS
        public static long Min(this long[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            long min = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }

        public static long Min(this long[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Min(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            long min = arr[0];

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


        // BYTES
        public static byte Min(this byte[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            byte min = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }

        public static byte Min(this byte[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Min(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            byte min = arr[0];

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
