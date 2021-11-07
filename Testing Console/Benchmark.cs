using DataScience;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using DataScience.Geometric;
using System.Collections.Generic;
using ILGPU;

namespace Testing_Console
{
    [MemoryDiagnoser]
    public class Benchmark
    {

        public static GPU gpu = new GPU();

        [Params(12,120, 1200,12000,120000,1200000)]
        public int length;
        public static Vector vector;


        [Benchmark]
        public void ToStr()
        {
            vector = Vector.Arange(gpu, -12, length, 1, 1, false);
            vector.ToString();
        }

        //[Benchmark]
        //public void ToStrNewer()
        //{
        //    vector = Vector.Arange(gpu, -12, length, 1, 1, false);
        //    vector.ToStringNEWER();
        //}

        //[Benchmark]
        //public void ToStrNewer2()
        //{
        //    vector = Vector.Arange(gpu, -12, length, 1, 1, false);
        //    vector.ToStringNEWER2();
        //}

        //[Benchmark]
        //public void ToStrNEWEST()
        //{
        //    vector = Vector.Arange(gpu, -12, length, 1, 1, false);
        //    vector.ToStringNEWEST();
        //}



    }



}
