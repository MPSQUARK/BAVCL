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

        public string ToString(byte decimalplaces = 2)
        {
            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"
            this.SyncCPU();
            int high = ((int)Util.Max(this.Value, true)).ToString().Length;
            int low = (int)Util.Min(this.Value, true);
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
                    spaces = new string(' ', digits - 3);
                }
                else if (float.IsNaN(Value[0]))
                {
                    val = "NaN   ";
                    spaces = new string(' ', digits - 3);
                }
                else
                {
                    val = Math.Abs(Value[0]).ToString(format);
                    spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                }
                if (Value[0] < 0f) { neg = '-'; } else { neg = ' '; }
                stringBuilder.Append($"|  {neg}{spaces}{val}  |");

                for (int i = 1; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    if (float.IsNegativeInfinity(Value[i]) || float.IsPositiveInfinity(Value[i]))
                    {
                        val = "INF   ";
                        spaces = new string(' ', digits - 3);
                    }
                    else if (float.IsNaN(Value[i]))
                    {
                        val = "NaN   ";
                        spaces = new string(' ', digits - 3);
                    }
                    else
                    {
                        val = Math.Abs(Value[i]).ToString(format);
                        spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                    }
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
    
        public string ToStringNEWER(byte decimalplaces = 2)
        {
            this.SyncCPU();
            int high = ((int)Util.Max(this.Value, true)).ToString().Length;
            int low = (int)Util.Min(this.Value, true);
            bool hasnegative = low < 0f;
            low = hasnegative ? low.ToString().Length - 1 : low.ToString().Length;
            bool hasinfinity = Value.Contains(float.PositiveInfinity) || Value.Contains(float.NegativeInfinity) || Value.Contains(float.NaN);
            int digits = high > low ? high : low;
            string format = $"F{decimalplaces}";

            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"
            char[] Template = new char[digits + decimalplaces + 7];
            Template[0] = '|';
            Template[1] = ' ';
            Template[2] = ' ';
            Template[^3] = ' ';
            Template[^2] = ' ';
            Template[^1] = '|';

            int diff = digits + decimalplaces + 1;

            StringBuilder stringBuilder = new StringBuilder();

            if (hasinfinity)
            {
                string neginf = $"|  {new string(' ', digits - 3)}-INF{new string(' ', decimalplaces)}  |";
                
                string inf = $"|  {new string(' ', digits - 2)}INF{new string(' ', decimalplaces)}  |";
                string nan = $"|  {new string(' ', digits - 2)}NaN{new string(' ', decimalplaces)}  |";


                for (int i = 0; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    Template[3] = Value[i] < 0f ? '-' : ' ';

                    if (float.IsFinite(this.Value[i]))
                    {
                        char[] val = Math.Abs(this.Value[i]).ToString(format).ToCharArray();

                        for (int j = 4; j < 4 + diff - val.Length; j++)
                        {
                            Template[j] = ' ';
                        }
                        val.CopyTo(Template, 4 + diff - val.Length);

                        stringBuilder.Append(Template);
                        continue;
                    }

                    if (float.IsPositiveInfinity(this.Value[i]))
                    {
                        stringBuilder.Append(inf);
                        continue;
                    }

                    if (float.IsNaN(this.Value[i]))
                    {
                        stringBuilder.Append(nan);
                        continue;
                    }

                    if (float.IsNegativeInfinity(this.Value[i]))
                    {
                        stringBuilder.Append(neginf);
                        continue;
                    }

                }

                return stringBuilder.ToString();
            }

            if (decimalplaces == 0) { digits--; }

            if (hasnegative)
            {

                for (int i = 0; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    Template[3] = Value[i] < 0f ? '-' : ' ';

                    if (float.IsFinite(this.Value[i]))
                    {
                        char[] val = Math.Abs(this.Value[i]).ToString(format).ToCharArray();

                        for (int j = 4; j < 4 + diff - val.Length; j++)
                        {
                            Template[j] = ' ';
                        }
                        val.CopyTo(Template, 4 + diff - val.Length);

                        stringBuilder.Append(Template);
                        continue;
                    }
                }

                return stringBuilder.ToString();
            }

            Template[3] = ' ';

            for (int i = 0; i < _length; i++)
            {
                if (i % Columns == 0) { stringBuilder.AppendLine(); }

                char[] val = this.Value[i].ToString(format).ToCharArray();

                for (int j = 4; j < 4 + diff - val.Length; j++)
                {
                    Template[j] = ' ';
                }
                val.CopyTo(Template, 4 + diff - val.Length);

                stringBuilder.Append(Template);

            }

            return stringBuilder.ToString();
        }


        public string ToStringNEWER2(byte decimalplaces = 2)
        {
            this.SyncCPU();
            int high = ((int)Util.Max(this.Value, true)).ToString().Length;
            int low = (int)Util.Min(this.Value, true);
            bool hasnegative = low < 0f;
            bool hasinfinity = Value.Contains(float.PositiveInfinity) || Value.Contains(float.NegativeInfinity) || Value.Contains(float.NaN);
            low = hasnegative ? low.ToString().Length - 1 : low.ToString().Length;
            int digits = high > low ? high : low;
            string format = $"F{decimalplaces}";

            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"
            char[] Template = new char[digits + decimalplaces + 7];
            Template[0] = '|';
            Template[1] = ' ';
            Template[2] = ' ';
            Template[^3] = ' ';
            Template[^2] = ' ';
            Template[^1] = '|';

            StringBuilder stringBuilder = new StringBuilder();
            string inf = new string(' ', digits - 3);
            string afterinf = new string(' ', decimalplaces + 1);
            string nan = new string(' ', decimalplaces);
            int _diff = digits + decimalplaces + 1;

            if (hasinfinity)
            {

                for (int i = 0; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    Template[3] = Value[i] < 0f ? '-' : ' ';

                    if (float.IsFinite(this.Value[i]))
                    {
                        char[] val = Math.Abs(this.Value[i]).ToString(format).ToCharArray();
                        val.CopyTo(Template, 3 + _diff - val.Length);

                        stringBuilder.Append(Template);
                        continue;
                    }

                    if (float.IsPositiveInfinity(this.Value[i]))
                    {
                        stringBuilder.Append($"|  {inf}INF{afterinf}  |");
                        continue;
                    }

                    if (float.IsNaN(this.Value[i]))
                    {
                        stringBuilder.Append($"|  {inf}NaN{afterinf}  |");
                        continue;
                    }

                    if (float.IsNegativeInfinity(this.Value[i]))
                    {
                        stringBuilder.Append($"|  {inf}-INF{nan}  |");
                        continue;
                    }

                }
            }

            if (hasnegative)
            {
                for (int i = 0; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    Template[3] = Value[i] < 0f ? '-' : ' ';

                    char[] val = Math.Abs(this.Value[i]).ToString(format).ToCharArray();
                    val.CopyTo(Template, 3 + _diff - val.Length);

                    stringBuilder.Append(Template);

                }
            }

            for (int i = 0; i < _length; i++)
            {
                if (i % Columns == 0) { stringBuilder.AppendLine(); }

                Template[3] = Value[i] < 0f ? '-' : ' ';

                char[] val = this.Value[i].ToString(format).ToCharArray();
                val.CopyTo(Template, 3 + _diff - val.Length);

                stringBuilder.Append(Template);

            }

            return stringBuilder.ToString();
        }
    }
}
