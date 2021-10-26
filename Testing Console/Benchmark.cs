using DataScience;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using DataScience.Geometric;
using System.Collections.Generic;

namespace Testing_Console
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        public GPU gpu = new GPU();

        [Params(10,100,1000,10000,100000,1000000,10000000)]
        public int length { get; set; }

        //public int length = 16;
        public float val = 5f;

        //[Benchmark]
        //public void Printv0()
        //{
        //    Vector vector = Vector.Arange(gpu, 0, length, 1, 1, false);
        //    vector.ToStringOrig();
        //}

        //[Benchmark]
        //public void Printv1()
        //{
        //    Vector vector = Vector.Arange(gpu, 0, length, 1, 1, false);
        //    vector.ToStringUpdate();
        //}

        [Benchmark]
        public void Printv2()
        {
            Vector vector = Vector.Arange(gpu, 0, length, 1, 1, false);
            vector.ToString();
        }

    }



}
