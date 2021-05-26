using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience.Utility
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







    }



}
