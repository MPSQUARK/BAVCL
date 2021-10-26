using ILGPU.Algorithms;
using System;
using System.Linq;
using System.Text;

namespace DataScience.Utility
{
    public class Util
    {

        public static bool IsClose(float val1, float val2, float threshold = 1e-5f)
        {
            return XMath.Abs(val1 - val2) < threshold ? true : false;
        }

        public static string ToString(float[] array, int Columns = 1, byte decimalplaces = 2)
        {
            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"

            int high = ((int)array.Max()).ToString().Length;
            int low = (int)array.Min();
            bool hasnegative = low < 0f;
            bool hasinfinity = array.Contains(float.PositiveInfinity) || array.Contains(float.NegativeInfinity) || array.Contains(float.NaN);
            low = hasnegative ? low.ToString().Length - 1 : low.ToString().Length;
            int digits = high > low ? high : low;

            string format = $"F{decimalplaces}";
            char neg = ' ';
            string val;
            string spaces;

            StringBuilder stringBuilder = new StringBuilder();



            stringBuilder.AppendLine();

            if (hasinfinity)
            {

                if (float.IsNegativeInfinity(array[0]) || float.IsPositiveInfinity(array[0]))
                {
                    val = "INF   ";
                }
                else if (float.IsNaN(array[0]))
                {
                    val = "NaN  ";
                }
                else
                {
                    val = Math.Abs(array[0]).ToString(format);
                }
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                if (array[0] < 0f) { neg = '-'; } else { neg = ' '; }
                stringBuilder.Append($"|  {neg}{spaces}{val}  |");

                for (int i = 1; i < array.Length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    if (float.IsNegativeInfinity(array[i]) || float.IsPositiveInfinity(array[i]))
                    {
                        val = "INF   ";
                    }
                    else if (float.IsNaN(array[i]))
                    {
                        val = "NaN   ";
                    }
                    else
                    {
                        val = Math.Abs(array[i]).ToString(format);
                    }
                    spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                    if (array[i] < 0f) { neg = '-'; } else { neg = ' '; }
                    stringBuilder.Append($"|  {neg}{spaces}{val}  |");
                }

                return stringBuilder.AppendLine().ToString();
            }

            if (hasnegative)
            {
                val = Math.Abs(array[0]).ToString(format);
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                if (array[0] < 0f) { neg = '-'; } else { neg = ' '; }
                stringBuilder.Append($"|  {neg}{spaces}{val}  |");

                for (int i = 1; i < array.Length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    val = Math.Abs(array[i]).ToString(format);
                    spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                    if (array[i] < 0f) { neg = '-'; } else { neg = ' '; }
                    stringBuilder.Append($"|  {neg}{spaces}{val}  |");
                }

                return stringBuilder.AppendLine().ToString();
            }

            val = array[0].ToString(format);
            spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
            stringBuilder.Append($"|  {neg}{spaces}{val}  |");

            for (int i = 1; i < array.Length; i++)
            {
                if (i % Columns == 0) { stringBuilder.AppendLine(); }

                val = array[i].ToString(format);
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                stringBuilder.Append($"|   {spaces}{val}  |");
            }

            return stringBuilder.AppendLine().ToString();

        }

        public static float Max(float[] arr)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            float max = arr[0];

            for (int i = 0; i < arr.Length; i++)
            {
                if (max < arr[i])
                {
                    max = arr[i];
                }
            }

            return max;
        }

        public static float Max(float[] arr, bool NonInf = true)
        {
            if (arr.Length == 0) { throw new Exception("Cannot Be Length 0"); }

            float max = arr[0];

            if (NonInf)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    if (float.PositiveInfinity == arr[i] || float.NegativeInfinity == arr[i])
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

            for (int i = 0; i < arr.Length; i++)
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
                for (int i = 0; i < arr.Length; i++)
                {
                    if (float.PositiveInfinity == arr[i] || float.NegativeInfinity == arr[i])
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

            for (int i = 0; i < arr.Length; i++)
            {
                if (min > arr[i])
                {
                    min = arr[i];
                }
            }

            return min;
        }


    }



}
