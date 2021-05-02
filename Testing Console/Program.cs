using System;
using DataScience;
using ILGPU;
using ILGPU.Runtime;
using System.Linq;

using System.Diagnostics;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();

            // SAMPLE AND TEST CODE


            Vector vectorA = Vector.Linspace(gpu, -100, -1, 100);
            Vector vectorB = Vector.Linspace(gpu, -100, 100, 200);
            Vector vectorC = Vector.Linspace(gpu, 100000000, 250000000, 100);
            Vector vectorD = Vector.Linspace(gpu, -100000000, -250000000, 25);
            Vector vectorE = Vector.Linspace(gpu, 100, -100, 201);
            Vector vectorF = Vector.Linspace(gpu, 642.21f, 8412.1f, 55);

            //Vector vectorB = Vector.Arange(-10, 10, 5);

            //Vector vectorD = Vector.Linspace(-5, 5, 10);
            vectorA.Columns = 10;
            vectorB.Columns = 10;
            vectorC.Columns = 5;
            vectorD.Columns = 5;
            vectorE.Columns = 10;
            vectorF.Columns = 11;

            //vectorD.Columns = 5;

            //vectorB.Print();

            Console.Write(vectorB.ToCSV());
            Console.WriteLine();

            Console.WriteLine(string.Join("", Enumerable.Repeat("a", 5)));

            Console.Write(vectorD.ToString());


            vectorA.Print();

            Vector vectorA_absW = Vector.AbsX(new Vector(gpu, new float[] { 1f }));

            vectorB.Print();
            vectorC.Print();

            vectorB.Print();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Vector vectorA_abs = Vector.Abs(vectorA);
            Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            sw.Stop();

            sw.Reset();
            sw.Start();
            Vector vectorA_abs2 = Vector.AbsX(vectorA);
            Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            sw.Stop();

            vectorA_abs2.Print();

            Console.WriteLine();
            Console.WriteLine($"Minimum in Abs : {vectorA_abs.Value.Min()} \nMinimum in Abs2 : {vectorA_abs2.Value.Min()}");



        }
    }
}
