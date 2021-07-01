using DataScience;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using DataScience.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();

            // SAMPLE AND TEST CODE

            Console.WriteLine();

            Vector[] vectors = new Vector[20];

            for (int i = 0; i < 20; i++)
            {
                vectors[i] = Vector.Arange(gpu, 0, 1e8f, 1f, 5);
                gpu.ShowMemoryUsage();
            }


            Console.WriteLine("Vectors Made, now DeCaching them \n\n\n");

            for (int i = 0; i < 20; i++)
            {
                gpu.DeCache((uint)vectors[i].Id);
                gpu.ShowMemoryUsage();
            }


           
        }






    }
}
