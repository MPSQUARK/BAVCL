using System;

namespace BAVCL.Ext
{
    public static partial class Extensions
    {
        // FLOATS
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


        // DOUBLES
        public static double Max(this double[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            double max = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }

        public static double Max(this double[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Max(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            double max = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (double.IsInfinity(arr[i])) { continue; }
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }


        // INTS
        public static int Max(this int[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            int max = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }

        public static int Max(this int[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Max(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            int max = arr[0];

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


        // LONGS
        public static long Max(this long[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            long max = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }

        public static long Max(this long[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Max(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            long max = arr[0];

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


        // BYTES
        public static byte Max(this byte[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            byte max = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }

        public static byte Max(this byte[] arr, bool ignoreInf = true)
        {
            if (!ignoreInf) { return arr.Max(); }

            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            byte max = arr[0];

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
