using DataScience.Utility;
using ILGPU.Algorithms;
using System;
using System.Linq;
using System.Text;

namespace DataScience
{
    public partial class Vector
    {


        public override string ToString()
        {
            return ToString(2);
        }
        public string ToString(byte decimalplaces = 2 )
        {
            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"

            int high = ((int)Max()).ToString().Length;
            int low = (int)Min();
            bool hasnegative = low < 0f;
            bool hasinfinity = Value.Contains(float.PositiveInfinity) || Value.Contains(float.NegativeInfinity) || Value.Contains(float.NaN);
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

                if (float.IsNegativeInfinity(Value[0]) || float.IsPositiveInfinity(Value[0]))
                {
                    val = "INF   ";
                }
                else if (float.IsNaN(Value[0]))
                {
                    val = "NaN  ";
                }
                else
                {
                    val = Math.Abs(Value[0]).ToString(format);
                }
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                if (Value[0] < 0f) { neg = '-'; } else { neg = ' '; }
                stringBuilder.Append($"|  {neg}{spaces}{val}  |");

                for (int i = 1; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    if (float.IsNegativeInfinity(Value[i]) || float.IsPositiveInfinity(Value[i]))
                    {
                        val = "INF   ";
                    }
                    else if (float.IsNaN(Value[i]))
                    {
                        val = "NaN   ";
                    }
                    else
                    {
                        val = Math.Abs(Value[i]).ToString(format);
                    }
                    spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                    if (Value[i] < 0f) { neg = '-'; } else { neg = ' '; }
                    stringBuilder.Append($"|  {neg}{spaces}{val}  |");
                }

                return stringBuilder.AppendLine().ToString();
            }

            if (hasnegative)
            {
                val = Math.Abs(Value[0]).ToString(format);
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                if (Value[0] < 0f) { neg = '-'; } else { neg = ' '; }
                stringBuilder.Append($"|  {neg}{spaces}{val}  |");

                for (int i = 1; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    val = Math.Abs(Value[i]).ToString(format);
                    spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                    if (Value[i] < 0f) { neg = '-'; } else { neg = ' '; }
                    stringBuilder.Append($"|  {neg}{spaces}{val}  |");
                }

                return stringBuilder.AppendLine().ToString();
            }

            val = Value[0].ToString(format);
            spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
            stringBuilder.Append($"|  {neg}{spaces}{val}  |");

            for (int i = 1; i < _length; i++)
            {
                if (i % Columns == 0) { stringBuilder.AppendLine(); }

                val = Value[i].ToString(format);
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                stringBuilder.Append($"|   {spaces}{val}  |");
            }

            return stringBuilder.AppendLine().ToString();

        }

    }
}
