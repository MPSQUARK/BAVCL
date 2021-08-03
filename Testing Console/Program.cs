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

            Vector testvecA = new Vector(gpu, new float[10] { 4, 9, 1, 6, 2, 7, 3, 8, 9, 1 });
            //Vector testvecB = new Vector(gpu, new float[10] { 4, 9, 1, 6, 2, 7, 3, 8, 9, 1 });

            testvecA.Print();
            testvecA.Diff_IP();
            testvecA.Print();

            Console.WriteLine(testvecA.Id);



            Console.Read();











        }






    }
}
