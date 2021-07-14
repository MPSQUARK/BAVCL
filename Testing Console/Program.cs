using DataScience;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using DataScience.IO;
using System.Threading.Tasks;
using System.Linq;

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

            float[] array = new float[(int)1e8];

            for (int i = 0; i < (int)1e8; i++)
            {
                array[i] = rnd.Next(0, 4);
            }

            Vector vec = new Vector(gpu, array, 1, true);

            float result = 0;
            float time = 0;

            //gpu.ShowMemoryUsage();
            //Vector vec = Vector.Arange(gpu, 0, 1e8f, 1f, 1);


            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10; i++)
            {
                result = vec.Std();
                //gpu.ShowMemoryUsage();
            }
            sw.Stop();
            time = sw.ElapsedMilliseconds;

            sw.Restart();
            Console.WriteLine($"\n\nresult: {result} | time : {time / 10 } ms");



            sw.Start();
            for (int i = 0; i < 10; i++)
            {
                result = vec.Std();
                //gpu.ShowMemoryUsage();
            }
            sw.Stop();
            time = sw.ElapsedMilliseconds;

            sw.Restart();
            Console.WriteLine($"\n\nresult: {result} | time : {time / 10 } ms");












            Console.Read();











        }






    }
}
