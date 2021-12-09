using ILGPU.Algorithms;

namespace BAVCL
{
    public partial class Vector
    {

        public static Vector Arange(GPU gpu, float startval, float endval, float interval, int Columns = 1, bool cache = true)
        {
            int steps = (int)((endval - startval) / interval);
            if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }
            if (endval % interval != 0) { steps++; }

            float[] values = new float[steps];

            for (int i = 0; i < steps; i++)
            {
                values[i] = startval + (i * interval);
            }

            return new Vector(gpu, values, Columns, cache);
        }


        public static float[] Arange(float startval, float endval, float interval)
        {
            int steps = (int)((endval - startval) / interval);
            if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }
            if (endval % interval != 0) { steps++; }

            float[] values = new float[steps];

            for (int i = 0; i < steps; i++)
            {
                values[i] = startval + (i * interval);
            }

            return values;
        }


    }
}
