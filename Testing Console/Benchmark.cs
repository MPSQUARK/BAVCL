using DataScience;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using ILGPU.Algorithms;
using System.Threading.Tasks;

namespace Testing_Console
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        GPU gpu = new GPU();
        
        float endval = 10000f;
        float interval = 0.7312f;

        [Benchmark]
        public Vector Arange()
        {
            float startval = 0f;

            int steps = (int)((endval - startval) / interval);
            if (endval < startval && interval > 0) { steps = Math.Abs(steps); interval = -interval; }
            if (endval % interval != 0) { steps++; }

            return new Vector(gpu, (from val in Enumerable.Range(0, steps)
                                    select startval + (val * interval)).ToArray(), 1, false);
        }

        //[Benchmark]
        //public void ArangeFor()
        //{

        //    int steps = (int)((endval - startval) / interval);
        //    if (endval < startval && interval > 0) { steps = Math.Abs(steps); interval = -interval; }
        //    if (endval % interval != 0) { steps++; }

        //    float[] vals = new float[steps];

        //    for (int i = 0; i < steps; i++)
        //    {
        //        vals[i] = startval + (i * interval);
        //    }

        //}

        [Benchmark]
        public Vector ArangeForWithXMath()
        {
            float startval = 0f;

            int steps = (int)((endval - startval) / interval);
            if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }
            if (endval % interval != 0) { steps++; }

            float[] vals = new float[steps];

            for (int i = 0; i < steps; i++)
            {
                vals[i] = startval + (i * interval);
            }
            return new Vector(gpu, vals, 1, false);
        }


        //[Benchmark]
        //public void ArangeForXMathAbsRcp()
        //{

        //    int steps = (int)((endval - startval) * XMath.Rcp(interval));

        //    if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }

        //    if (endval % interval != 0) { steps++; }

        //    float[] vals = new float[steps];

        //    for (int i = 0; i < steps; i++)
        //    {
        //        vals[i] = startval + (i * interval);
        //    }

        //}

        //[Benchmark]
        //public void ArangeForXMathRcp()
        //{

        //    int steps = (int)((endval - startval) * XMath.Rcp(interval));

        //    if (endval < startval && interval > 0) { steps = Math.Abs(steps); interval = -interval; }

        //    if (endval % interval != 0) { steps++; }

        //    float[] vals = new float[steps];

        //    for (int i = 0; i < steps; i++)
        //    {
        //        vals[i] = startval + (i * interval);
        //    }

        //}

        //[Benchmark]
        //public void ArangeForXMathRcpParallel()
        //{

        //    int steps = (int)((endval - startval) * XMath.Rcp(interval));

        //    if (endval < startval && interval > 0) { steps = Math.Abs(steps); interval = -interval; }

        //    if (endval % interval != 0) { steps++; }

        //    float[] vals = new float[steps];


        //    Parallel.For(0, vals.Length, new ParallelOptions { MaxDegreeOfParallelism = Threads }, i =>
        //    {
        //        vals[i] = startval + (i * interval);
        //    });

        //}





    }



}
