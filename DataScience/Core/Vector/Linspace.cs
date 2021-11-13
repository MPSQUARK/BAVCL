using ILGPU.Algorithms;
using System;
using System.Linq;

namespace BAVCL
{
    public partial class Vector
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startval"></param>
        /// <param name="endval"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        //public static Vector Linspace(GPU gpu, float startval, float endval, int steps, int Columns = 1, bool cache = true)
        //{
        //    if (steps <= 1) { throw new Exception("Cannot make linspace with less than 1 steps"); }
        //    float interval = (endval - startval) / (steps - 1);
        //    return new Vector(gpu, (from val in Enumerable.Range(0, steps) select startval + (val * interval)).ToArray(), Columns, cache);
        //}

        public static Vector Linspace(GPU gpu, float startval, float endval, int steps, int Columns = 1, bool cache = true)
        {
            if (steps <= 1) { throw new Exception("Cannot make linspace with less than 1 steps"); }
            float interval = (endval - startval) / (steps - 1);

            float[] arr = new float[steps];

            for (int i = 0; i < steps; i++)
            {
                arr[i] = startval + (i * interval);
            }

            return new Vector(gpu, arr, Columns, cache);
        }

        public static float[] Linspace(float startval, float endval, int steps)
        {
            if (steps <= 1) { throw new Exception("Cannot make linspace with less than 1 steps"); }
            float interval = (endval - startval) / (steps - 1);

            float[] arr = new float[steps];

            for (int i = 0; i < steps; i++)
            {
                arr[i] = startval + (i * interval);
            }

            return arr;
        }



    }
}
