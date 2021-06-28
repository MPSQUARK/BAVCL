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
            Random rnd = new Random();
            Stopwatch sw = new Stopwatch();

            float result = 0f;
            float time = 0f;
            int len = 10;

            Console.WriteLine();
            //int[] testlengths = new int[] { 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000 };
            //int[] scale = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            //List<float> Resultslst = new List<float>();

            //for (int j = 0; j < testlengths.Length; j++)
            //{

            //    float ActualResult = ((testlengths[j] - 1) / 2f) * (testlengths[j]);
            //    Console.WriteLine($"Actual Result : {ActualResult}\n\n");
            //    Vector vector = Vector.Arange(gpu, 0, testlengths[j], 1, 1);


            //    Resultslst.Add(testlengths[j]);


            //    // Numerics
            //    sw.Start();
            //    for (int i = 0; i < len; i++)
            //    {
            //        result = vector.Sum2();
            //    }
            //    time = sw.ElapsedMilliseconds;
            //    sw.Stop();
            //    sw.Reset();
            //    Resultslst.Add((time * 1e-2f));
            //    Resultslst.Add((result - ActualResult));
            //    //Console.WriteLine($"result : {result}");
            //    //Console.WriteLine($"difference : {result-ActualResult}");

            //    // Kahan
            //    sw.Start();
            //    for (int i = 0; i < len; i++)
            //    {
            //        result = vector.Sum5();
            //    }
            //    time = sw.ElapsedMilliseconds;
            //    sw.Stop();
            //    sw.Reset();
            //    Resultslst.Add((time * 1e-2f));
            //    Resultslst.Add((result - ActualResult));
            //    //Console.WriteLine($"result : {result}");
            //    //Console.WriteLine($"difference : {result - ActualResult}");

            //    // Kahan mixed
            //    sw.Start();
            //    for (int i = 0; i < len; i++)
            //    {
            //        result = vector.Sum6();
            //    }
            //    time = sw.ElapsedMilliseconds;
            //    sw.Stop();
            //    sw.Reset();
            //    Resultslst.Add((time * 1e-2f));
            //    Resultslst.Add((result - ActualResult));
            //    //Console.WriteLine($"result : {result}");
            //    //Console.WriteLine($"difference : {result - ActualResult}");

            //    //GPU

            //    if (testlengths[j] >= 1e5)
            //    {
            //        sw.Start();
            //        for (int i = 0; i < len; i++)
            //        {
            //            result = vector.Sum4();
            //        }
            //        time = sw.ElapsedMilliseconds;
            //        sw.Stop();
            //        sw.Reset();
            //        Resultslst.Add((time * 1e-2f));
            //        Resultslst.Add((result - ActualResult));
            //        //Console.WriteLine($"result : {result}");
            //        //Console.WriteLine($"difference : {result / ActualResult}");
            //    }
            //    else
            //    {
            //        Resultslst.Add((-999));
            //        Resultslst.Add((-999));
            //    }

            //    //Console.ReadLine();

            //}


            //Vector Results = new Vector(gpu, Resultslst.ToArray(), 9);
            //IO.WriteToCSV(Results, "Benchmark_27_06-4");

            //Vector vector = Vector.Arange(gpu, 0, 10000000, 1, 1);

            //sw.Start();
            //for (int i = 0; i < len; i++)
            //{
            //    result = vector.Var();
            //}
            //time = sw.ElapsedMilliseconds;
            //sw.Stop();
            //sw.Reset();
            //Console.WriteLine($"Time : {time * 1e-2f}");
            //Console.WriteLine($"Result : {result}\n");

            //sw.Start();
            //for (int i = 0; i < len; i++)
            //{
            //    result = vector.Var2();
            //}
            //time = sw.ElapsedMilliseconds;
            //sw.Stop();
            //sw.Reset();
            //Console.WriteLine($"Time : {time * 1e-2f}");
            //Console.WriteLine($"Result : {result}\n");




        }






    }
}
