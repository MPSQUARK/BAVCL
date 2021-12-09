using BAVCL.Utility;
using System;
using System.Text;

namespace BAVCL.Core
{
    public static partial class Extensions
    {
        public static string ToStr(this float[] arr, byte decimalplaces = 2)
        {
            (float min, float max, bool hasinfinity) = Util.MinMaxInf(arr);

            bool hasnegative = min < 0f;

            int high = max.ToString().Length;
            int low = hasnegative ? min.ToString().Length - 1 : min.ToString().Length;

            int digits = high > low ? high : low;

            string format = $"F{decimalplaces}";

            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"
            char[] Template = new char[digits + decimalplaces + 7];

            Template[0] =  '|';  // Padding
            Template[1] =  ' ';  // Padding
            Template[2] =  ' ';  // Negative Sign
            Template[^4] = ' ';  // Padding
            Template[^3] = ' ';  // Padding
            Template[^2] = '|';  // Padding
            Template[^1] = '\n'; // New Line

            StringBuilder stringBuilder = new StringBuilder();
            char[] clear = new string(' ', Template.Length - 6).ToCharArray();
            int _diff = digits + 4 + decimalplaces;

            if (hasinfinity)
            {
                string inf = new string(' ', digits - 3);
                string afterinf = new string(' ', decimalplaces + 1);
                string nan = new string(' ', decimalplaces);


                for (int i = 0; i < arr.Length; i++)
                {

                    Template[2] = arr[i] < 0f ? '-' : ' ';

                    if (float.IsFinite(arr[i]))
                    {
                        clear.CopyTo(Template, 3);
                        string val = Math.Abs(arr[i]).ToString(format);
                        val.CopyTo(0, Template, _diff - val.Length, val.Length);

                        stringBuilder.Append(Template);
                        continue;
                    }

                    if (float.IsPositiveInfinity(arr[i]))
                    {
                        stringBuilder.Append($"|  {inf}INF{afterinf} |\n");
                        continue;
                    }

                    if (float.IsNaN(arr[i]))
                    {
                        stringBuilder.Append($"|  {inf}NaN{afterinf} |\n");
                        continue;
                    }

                    if (float.IsNegativeInfinity(arr[i]))
                    {
                        stringBuilder.Append($"| -{inf}INF{nan}  |\n");
                        continue;
                    }

                }
                return stringBuilder.ToString();
            }

            if (hasnegative)
            {
                for (int i = 0; i < arr.Length; i++)
                {

                    Template[2] = arr[i] < 0f ? '-' : ' ';

                    clear.CopyTo(Template, 3);
                    string val = Math.Abs(arr[i]).ToString(format);
                    val.CopyTo(0, Template, _diff - val.Length, val.Length);

                    stringBuilder.Append(Template);
                    continue;
                }

                return stringBuilder.ToString();
            }

            for (int i = 0; i < arr.Length; i++)
            {

                clear.CopyTo(Template, 3);
                string val = arr[i].ToString(format);
                val.CopyTo(0, Template, _diff - val.Length, val.Length);

                stringBuilder.Append(Template);
                continue;

            }

            return stringBuilder.ToString();
        }


    }


}
