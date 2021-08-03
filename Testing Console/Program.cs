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

            //Vector testvecA = new Vector(gpu, new float[10] );
            //Vector testvecB = new Vector(gpu, new float[10] );

            //for (int i = 0; i < testvecA.Value.Length; i++)
            //{
            //    testvecA.Value[i] = rnd.Next(1, 6);
            //}

            Vector testvecB = new Vector(gpu, new float[5] {5,4,9,1,3 });
            Vector vector = Vector.Normalise(testvecB);
            vector.Print();

            Vector testvecA = testvecB.Copy();

            testvecB.Normalise_IP2();
            testvecB.Print();

            testvecA.Print();


            float time = 0f;
            int runs = 50000;


            Stopwatch sw = new();

            //Warm up
            for (int i = 0; i < 100; i++)
            {
                testvecB = testvecA.Copy();
                testvecB.Normalise_IP();
            }

            sw.Start();
            for (int i = 0; i < runs; i++)
            {
                testvecB = testvecA.Copy();
                testvecB.Normalise_IP();
            }
            time = sw.ElapsedMilliseconds;
            Console.WriteLine($"Old normalise function time is : {time/runs} ms");


            // Warm up
            for (int i = 0; i < 100; i++)
            {
                testvecB = testvecA.Copy();
                testvecB.Normalise_IP2();
            }


            sw.Restart();

            for (int i = 0; i < runs; i++)
            {
                testvecB = testvecA.Copy();
                testvecB.Normalise_IP2();
            }
            time = sw.ElapsedMilliseconds;
            Console.WriteLine($"NEW normalise function time is : {time/runs} ms");


            // Warm up
            for (int i = 0; i < 100; i++)
            {
                testvecB = testvecA.Copy();
                testvecB.Normalise_IP3();
            }


            sw.Restart();

            for (int i = 0; i < runs; i++)
            {
                testvecB = testvecA.Copy();
                testvecB.Normalise_IP3();
            }
            time = sw.ElapsedMilliseconds;
            Console.WriteLine($"NEW-XMATH normalise function time is : {time / runs} ms");



            sw.Stop();


            Console.Read();


            








        }






    }
}
