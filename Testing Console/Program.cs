using DataScience;
using DataScience.Core;
using System;
using System.Diagnostics;
using DataScience.Geometric;
using DataScience.Utility;
using System.Linq;
using BenchmarkDotNet.Running;
using ILGPU.Algorithms;
using System.Threading.Tasks;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU();
            Random rnd = new Random(522);

            BenchmarkRunner.Run<Benchmark>();

            //Benchmark benchmark = new();
            //float dist = benchmark.Distance();
            //float dist2 = benchmark.DistanceVec();


            //Console.WriteLine($"orig : {dist} | got : {dist2}");


            //Console.WriteLine(Util.ToString(new float[2]));


        }

       




    }
}
