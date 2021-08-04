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
                testvecA = Vector.Arange(gpu, 0, 20, 1f, 4);
                testvecA.Transpose_IP();
            }

            sw.Start();

            for (int i = 0; i < runs; i++)
            {
                testvecA = Vector.Arange(gpu, 0, 20, 1f, 4);
                testvecA.Transpose_IP();
            }
            time = sw.ElapsedMilliseconds;
            Console.WriteLine($"time taken for ReverseX_IP with two Inputs is {time/runs} ms");


            // 0.154 ~ 0.146 ms
            // 0.142 ~ 
            // 

            sw.Stop();

            testvecA = Vector.Arange(gpu, 0, 16, 1f, 4);
            testvecA.Print();
            testvecA.Transpose_IP();
            testvecA.Print();

            Console.Read();


            








        }






    }
}
