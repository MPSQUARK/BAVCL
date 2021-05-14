using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{

    namespace Utility
    {
        public class Util
        {
            public static string PadBoth(string source, int length, int disp, bool neg)
            {
                disp = Math.Abs(disp);
                int spaces = length - source.Length + 2;
                int padLeft = (int)(spaces * 0.5f) + source.Length;
                string ws;

                if (neg)
                {
                    if (disp == 0) { disp++; }
                    ws = string.Join("", Enumerable.Repeat(" ", disp - 1));
                    return (ws + source).PadLeft(padLeft - disp).PadRight(length);
                }

                ws = string.Join("", Enumerable.Repeat(" ", disp));
                return (ws + source).PadLeft(padLeft - disp).PadRight(length);
            }

            public static bool IsClose(float val1, float val2, float threshold = 1e-5f)
            {
                return MathF.Abs(val1 - val2) < threshold ? true : false;
            }


            // Might move to their corresponding Classes in future
            public static string ConvertToFormat(Vector vector, string format = "txt")
            {
                switch (format)
                {
                    case "txt":
                        return vector.ToString();
                    case "csv":
                        return vector.ToCSV();
                    default:
                        throw new Exception($"No format : {format} available\n Func - ConvertToFormat - IO");
                }
            }
            public static string ConvertToFormat(Vector3 vector, string format = "txt")
            {
                switch (format)
                {
                    case "txt":
                        return vector.ToString();
                    case "csv":
                        return vector.ToCSV();
                    default:
                        throw new Exception($"No format : {format} available\n Func - ConvertToFormat - IO");
                }
            }




        }


    }

}
