using BAVCL.Utility;
using System;
using System.Text;

namespace BAVCL.Core
{
    public static partial class Extensions
    {

        #region "FLOAT"

        public static void Print(this float value, byte decimalplaces = 2)
        {
            Console.WriteLine(value.ToString($"F{decimalplaces}"));
        }

        public static void Print(this float[] arr, byte decimalplaces = 2)
        {
            Console.WriteLine();
            Console.WriteLine(arr.ToStr(decimalplaces));
        }

        public static void Print(this float[,] arr, byte decimalplaces = 2)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region "DOUBLE"

        public static void Print(this double value, byte decimalplaces = 2)
        {
            Console.WriteLine(value.ToString($"F{decimalplaces}"));
        }

        public static void Print(this double[] arr, byte decimalplaces = 2)
        {
            throw new NotImplementedException();
        }

        public static void Print(this double[,] arr, byte decimalplaces = 2)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region "INTS"

        public static void Print(this int value)
        {
            Console.WriteLine(value.ToString());
        }

        public static void Print(this int[] arr)
        {
            throw new NotImplementedException();
        }

        public static void Print(this int[,] arr)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region "LONG"

        public static void Print(this long value)
        {
            Console.WriteLine(value.ToString());
        }

        public static void Print(this long[] arr)
        {
            throw new NotImplementedException();
        }

        public static void Print(this long[,] arr)
        {
            throw new NotImplementedException();
        }

        #endregion


        

    }


}
