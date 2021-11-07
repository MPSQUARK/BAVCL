using DataScience;
using DataScience.Core;
using System;

using BenchmarkDotNet.Running;

using DataScience.Ext;
using DataScience.Geometric;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //Random rnd = new Random(522);

            GPU gpu = new GPU();

            float[] arr = new float[] { -5, -4, -3, float.PositiveInfinity, -2, -1, float.NegativeInfinity, 0, 1, float.NaN, 2, 3, -12312, 4, 5 };

            Console.WriteLine(arr);

            arr.Print();

            Vector.Arange(gpu, -12, 12, 1, 4).Print();

            Vector3.Fill(gpu, 5, 12).Print();

            int val = 12412;
            val.Print();

        }






    }
}
