using DataScience;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using DataScience.IO;
using System.Threading.Tasks;
using System.Linq;

using ILGPU.Algorithms;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU();
            Random rnd = new Random();

            // SAMPLE AND TEST CODE

            Console.WriteLine();

            Vector testvecA;

            float time = 0f;
            int runs = 500;


            Stopwatch sw = new();

            //Warm up
            for (int i = 0; i < 100; i++)
            {
                testvecA = Vector.Arange(gpu, 0, 10, 1f, 1);
                testvecA.ReverseX_IP();
            }

            sw.Start();

            for (int i = 0; i < runs; i++)
            {
                testvecA = Vector.Arange(gpu, 0, 100, 0.5f, 1);
                testvecA.ReverseX_IP();
            }
            time = sw.ElapsedMilliseconds;
            Console.WriteLine($"time taken for ReverseX_IP with two Inputs is {time/runs} ms");


            // 0.150-0.160 ms  0%
            // 0.140-0.145 ms  6.7% ~ 9.4%
            // 0.108-0.114 ms  28%  ~ 28.75%

            sw.Stop();


            Console.Read();


            








        }






    }
}
