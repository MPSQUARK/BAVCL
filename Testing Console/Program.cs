using DataScience;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();

            // SAMPLE AND TEST CODE

            //SubtractionTest(gpu, 10_000_000);

            Vector3 vec = new Vector3(gpu, new float[] { 1, 2, 3, 4, 5, 6 });
            Vertex vert = new Vertex(99f, 99f, 99f);
            Vertex vert1 = new Vertex(88f, 88f, 88f);
            Vertex vert2 = new Vertex(999f, 999f, 999f);
            Vertex vert3 = new Vertex(998f, 998f, 998f);

            //vec.Print();
            //Vector3 vec2 = Vector3.AppendVert(vec, vert);
            //vec.Print();
            //vec2.Print();

            vec = new Vector3(gpu, new float[] { 1, 2, 3, 4, 5, 6 });
            vec.Print();
            Vector3 vec2 = Vector3.AppendVert(vec, new List<Vertex> { vert, vert1, vert2, vert3 });
            vec.Print();
            vec2.Print();


        }

        public static void SubtractionTest(GPU gpu, int size)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Vector a = Vector.Linspace(gpu, 0, 1, size, 1);
            Vector b = Vector.Linspace(gpu, 1, 0, size, 1);

            timer.Stop();
            TimeSpan constructionTime = timer.Elapsed;
            timer.Restart();

            a._ConsecutiveOP(10, Operations.multiplication);
            b._ConsecutiveOP(10, Operations.multiplication);

            a._ConsecutiveOP(b, Operations.subtraction);

            timer.Stop();
            TimeSpan calculationTime = timer.Elapsed;

            if (a.Value[0] == 0)
            {
                Console.WriteLine("SubtractionTest Passed");
            }
            else
            {
                Console.WriteLine("SubtractionTest Failed");
            }

            Console.WriteLine("Construction Time: " + constructionTime.TotalMilliseconds + " ms");
            Console.WriteLine("Calculation Time: " + calculationTime.TotalMilliseconds + " ms");
        }




    }
}
