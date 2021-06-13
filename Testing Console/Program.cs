using DataScience;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using DataScience.IO;


namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();

            // SAMPLE AND TEST CODE

            Vector vector = Vector.Arange(gpu, 20000,0, -0.5f, 5);
            //vector.Print();

            float time = 0f;
            int length = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < length; i++)
            {
                vector.Sum();
            }
            time = sw.ElapsedMilliseconds;
            sw.Stop();
            sw.Reset();

            Console.WriteLine($"Time taken is {time / length} ms per itteration");
            Console.WriteLine($"The value is {vector.Sum()}");



            sw.Start();
            for (int i = 0; i < length; i++)
            {
                vector.Var2();
            }
            time = sw.ElapsedMilliseconds;
            sw.Stop();
            sw.Reset();

            Console.WriteLine($"Time taken is {time / length} ms per itteration");
            Console.WriteLine($"The value is {vector.Var2()}");

        }






    }
}
