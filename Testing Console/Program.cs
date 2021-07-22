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

            Vector testvecA = Vector.Arange(gpu, 0, 10, 1, 1);
            Vector testvecB = Vector.Fill(gpu, 5f, testvecA.Length());

            float time = 0f;
            int reps = 20000;

            int[] repIntervals = new int[] { 1, 10, 100, 1000, 2000, 4000, 8000, 10000, 50000, 100000 };

            // WARM UP

            Stopwatch sw = new Stopwatch();
            
            for (int i = 0; i < reps; i++)
            {
                testvecA.ConsecutiveOP_IP(testvecB, Operations.multiplication);
            }
            time = sw.ElapsedMilliseconds;
            //Console.WriteLine($"[Cached] Total time taken is {time} ms : Avg per call is {time / reps} ms");

            testvecA = Vector.Arange(gpu, 0, 10, 1, 1);
            for (int i = 0; i < reps; i++)
            {
                testvecA.ConsecutiveOP_IP(5f, Operations.multiplication);
            }


            List<float> times = new List<float>();


            // WARM UP

            for (int j = 0; j < repIntervals.Length; j++)
            {
                reps = repIntervals[j];
                times.Add(reps);

                testvecA = Vector.Arange(gpu, 0, 10, 1, 1);

                sw.Start();

                for (int i = 0; i < reps; i++)
                {
                    testvecA.ConsecutiveOP_IP(testvecB, Operations.multiplication);
                }
                time = sw.ElapsedMilliseconds;
                Console.WriteLine($"[Cached] Total time taken for {reps} reps is {time} ms : Avg per call is {time / reps} ms");
                
                times.Add(time);
                times.Add(time / reps);
               

                sw.Stop();
                sw.Reset();

                testvecA = Vector.Arange(gpu, 0, 10, 1, 1);

                sw.Start();
                for (int i = 0; i < reps; i++)
                {
                    testvecA.ConsecutiveOP_IP(5f, Operations.multiplication);
                }
                time = sw.ElapsedMilliseconds;
                Console.WriteLine($"Total time taken for {reps} reps is {time} ms : Avg per call is {time / reps} ms");

                times.Add(time);
                times.Add(time / reps);

                sw.Stop();
                sw.Reset();



            }

            Vector TimesVec = new Vector(gpu, times.ToArray(), 5, false);
            IO.WriteToCSV(TimesVec, "LRU vs OLD");



            Console.Read();











        }






    }
}
