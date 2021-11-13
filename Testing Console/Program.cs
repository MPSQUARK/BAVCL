using BAVCL;
using BAVCL.Core;
using System;

using BenchmarkDotNet.Running;


using BAVCL.Geometric;
using ILGPU.Algorithms;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //Random rnd = new Random(522);

            GPU gpu = new GPU();
            Vector3 vecA = new Vector3(gpu, new float[] { 4f, 5f, 2f, 1f, 9f, 3f });
            Vector3 vecB = new Vector3(gpu, new float[] { 1f, 2f, 3f, 5f, 6f, 7f });

            Console.WriteLine();
            Console.WriteLine(MathF.Pow(9f, 6f));
            Console.WriteLine(Math.Pow(9f, 6f));

            Console.WriteLine();
            Console.WriteLine(XMath.Pow(9f, 6f));
            Console.WriteLine(XMath.Pow((double)9f, (double)6f));

            Console.WriteLine("\nOperation: +");
            (vecA + vecB).Print();

            Console.WriteLine("\nOperation: -");
            (vecA - vecB).Print();

            Console.WriteLine("\nOperation: *");
            (vecA * vecB).Print();

            Console.WriteLine("\nOperation: /");
            (vecA / vecB).Print();


            Vector3 vector3 = Vector3.OP(vecA, vecB, Operations.pow);

            Console.WriteLine("\nOperation: ^");
            (vector3).Print();



        }






    }
}
