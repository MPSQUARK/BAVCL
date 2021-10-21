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

            Vector vec = Vector.Arange(gpu,-12, 12, 1, 4);
            vec.SyncCPU();
            vec.Value[5] = float.PositiveInfinity;
            vec.Value[8] = float.NegativeInfinity;
            vec.Value[10] = float.NaN;
            vec.UpdateCache(vec.Value);
            Vector3 vector = vec.ToVector3();
            vector.Print();
            //Console.WriteLine(vec.ToStringOld());

            Console.Read();



        }






    }
}
