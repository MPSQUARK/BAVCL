using BAVCL.Utility;
using ILGPU.Algorithms;
using System;
using System.Linq;
using System.Text;


namespace BAVCL
{
    public partial class Vector
    {
        public override string ToString()
        {
            return ToStr(2,false);
        }

        public string ToStr(byte decimalplaces = 2, bool syncCPU = true)
        {
            if ((this.Value == null) || syncCPU) { this.SyncCPU(); }

            (float min, float max, bool hasinfinity) = Util.MinMaxInf(this.Value);

            bool hasnegative = min < 0f;

            int high = max.ToString().Length;
            int low = hasnegative ? min.ToString().Length - 1 : min.ToString().Length;

            int digits = high > low ? high: low;

            string format = $"F{decimalplaces}";

            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"
            char[] Template = new char[digits + decimalplaces + 6];

            Template[0] = '|';  // Padding
            Template[1] = ' ';  // Padding
            Template[2] = ' ';  // Negative Sign
            Template[^3] = ' '; // Padding
            Template[^2] = ' '; // Padding
            Template[^1] = '|'; // Padding

            StringBuilder stringBuilder = new();
            char[] clear = new string(' ', Template.Length - 6).ToCharArray();
            int _diff = digits + 4 + decimalplaces;

            if (hasinfinity)
            {
                string 
                    inf = new(' ', digits - 3),
                    afterinf = new(' ', decimalplaces + 1),
                    nan = new(' ', decimalplaces);


                for (int i = 0; i < Length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    Template[2] = Value[i] < 0f ? '-' : ' ';

                    if (float.IsFinite(this.Value[i]))
                    {
                        clear.CopyTo(Template, 3);
                        string val = Math.Abs(this.Value[i]).ToString(format);
                        val.CopyTo(0, Template, _diff - val.Length, val.Length);

                        stringBuilder.Append(Template);
                        continue;
                    }

                    if (float.IsPositiveInfinity(this.Value[i]))
                    {
                        stringBuilder.Append($"|  {inf}INF{afterinf} |");
                        continue;
                    }

                    if (float.IsNaN(this.Value[i]))
                    {
                        stringBuilder.Append($"|  {inf}NaN{afterinf} |");
                        continue;
                    }

                    if (float.IsNegativeInfinity(this.Value[i]))
                    {
                        stringBuilder.Append($"| -{inf}INF{nan}  |");
                        continue;
                    }

                }
                return stringBuilder.ToString();
            }

            if (hasnegative)
            {
                for (int i = 0; i < Length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    Template[2] = Value[i] < 0f ? '-' : ' ';

                    clear.CopyTo(Template, 3);
                    string val = Math.Abs(this.Value[i]).ToString(format);
                    val.CopyTo(0, Template, _diff - val.Length, val.Length);

                    stringBuilder.Append(Template);
                    continue;
                }

                return stringBuilder.ToString();
            }


            for (int i = 0; i < Length; i++)
            {
                if (i % Columns == 0) { stringBuilder.AppendLine(); }

                clear.CopyTo(Template, 3);
                string val = this.Value[i].ToString(format);
                val.CopyTo(0, Template, _diff - val.Length, val.Length);

                stringBuilder.Append(Template);
                continue;

            }

            return stringBuilder.ToString();
        }


    }
}
