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

            Vector vector = Vector.Arange(gpu, -12, 16, 1, 4, true);
            

            //Console.WriteLine(vector.ToStringOrig());
            //Console.WriteLine(vector.ToString());

            vector.SyncCPU();
            vector.Value[2] = float.PositiveInfinity;
            vector.Value[3] = float.NegativeInfinity;
            vector.Value[4] = float.NaN;
            vector.UpdateCache();
            vector.Print();

            //Console.WriteLine(vector.ToStringOrig());
            //Console.WriteLine(vector.ToStringUpdate());
            //Console.WriteLine(vector.ToString());

            //BenchmarkRunner.Run<Benchmark>();

            //Benchmark benchmark = new();
            //float[] dist = benchmark.Linspace().Value;
            //float[] dist2 = benchmark.LinspaceForloop().Value;
            //float[] dist3 = benchmark.LinspaceForloopHybrid().Value;

            //for (int i = 0; i < dist.Length; i++)
            //{
            //    Console.WriteLine($"orig : {dist[i]} | got : {dist2[i]} | v3 : {dist3[i]}");
            //}



            //Console.WriteLine(Util.ToString(new float[2]));


        }






    }
}
