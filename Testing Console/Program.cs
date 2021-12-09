using BAVCL;
//using BAVCL.Core;
using System;
using BenchmarkDotNet.Running;

using BAVCL.Experimental;
using BAVCL.Geometric;
using BAVCL.Core;
using System.Threading.Tasks;
//using BAVCL.Geometric;
//using ILGPU.Algorithms;
//using BAVCL.Plotting;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();


            Vector vector = new Vector(gpu, new float[2] { 1f,2f});

            vector.UpdateCache(new float[] { 6f,7f});

        }







    }

}