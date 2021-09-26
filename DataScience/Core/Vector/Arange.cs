using System;
using System.Linq;

namespace DataScience
{
    public partial class Vector
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startval"></param>
        /// <param name="endval"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Vector Arange(GPU gpu, float startval, float endval, float interval, int Columns = 1)
        {
            int steps = (int)((endval - startval) / interval);
            if (endval < startval && interval > 0) { steps = Math.Abs(steps); interval = -interval; }
            if (endval % interval != 0) { steps++; }

            return new Vector(gpu, (from val in Enumerable.Range(0, steps)
                                    select startval + (val * interval)).ToArray(), Columns);
        }

    }
}
