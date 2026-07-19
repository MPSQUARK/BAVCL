using System;
using BAVCL.Utility;
using System.Text;

namespace BAVCL.Geometric;


public partial class Vector3
{
    public override string ToString()
    {
        return ToString(2);
    }


    public string ToString(byte decimalplaces = 2)
    {
        ReadOnlySpan<float> data = RetrieveReadOnlySpan();
        (float min, float max, bool hasinfinity) = Util.MinMaxInf(data);

        bool hasnegative = min < 0f;

        int high = max.ToString().Length;
        int low = hasnegative ? min.ToString().Length - 1 : min.ToString().Length;

        int digits = high > low ? high : low;

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

                Template[2] = data[i] < 0f ? '-' : ' ';

                if (float.IsFinite(data[i]))
                {
                    clear.CopyTo(Template, 3);
                    string val = Math.Abs(data[i]).ToString(format);
                    val.CopyTo(0, Template, _diff - val.Length, val.Length);

                    stringBuilder.Append(Template);
                    continue;
                }

                if (float.IsPositiveInfinity(data[i]))
                {
                    stringBuilder.Append($"|  {inf}INF{afterinf} |");
                    continue;
                }

                if (float.IsNaN(data[i]))
                {
                    stringBuilder.Append($"|  {inf}NaN{afterinf} |");
                    continue;
                }

                if (float.IsNegativeInfinity(data[i]))
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

                Template[2] = data[i] < 0f ? '-' : ' ';

                clear.CopyTo(Template, 3);
                string val = Math.Abs(data[i]).ToString(format);
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
            string val = data[i].ToString(format);
            val.CopyTo(0, Template, _diff - val.Length, val.Length);

            stringBuilder.Append(Template);
            continue;

        }

        return stringBuilder.ToString();
    }


}