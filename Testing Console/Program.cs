using DataScience;
using System;
using System.Diagnostics;
using DataScience.Geometric;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU(memorycap:0.94f);
            Random rnd = new Random(522);

            Vector vec = Vector.Arange(gpu,0, 12000000, 1, 4);

            float time = 0f;
            int reps = 100;

            Stopwatch sw = new();

            vec.ToString();

            sw.Start();
            vec.ToString();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine($"time NEW : {time / reps} ms");



            vec.ToStringOld();

            sw.Restart();
            vec.ToStringOld();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine($"time OLD : {time/reps} ms");
            sw.Stop();

            Console.Read();



        }






    }
}
