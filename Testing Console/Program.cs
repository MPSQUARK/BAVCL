using DataScience;
using System;
using System.Diagnostics;
using DataScience.Geometric;
using ILGPU.Runtime;
using ILGPU.Algorithms;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU(memorycap:0.94f);
            Random rnd = new Random(522);


            // SAMPLE AND TEST CODE
            Vector vec = Vector.Linspace(gpu, 0f, 19f, 20, 5);
            vec.Print();

            Vector vec2 = Vector.Linspace(gpu, 0f, -19f, 20, 4);
            vec2.Print();

            Vector vecR = Vector.Concat(vec, vec2, 'c', true);
            vecR.Print();

            
            Console.Read();



        }






    }
}
