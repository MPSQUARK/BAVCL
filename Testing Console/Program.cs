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

            Vector vector = Vector.Arange(gpu, 0, 10, 1, 1);
            Vector vector1 = Vector.Arange(gpu, 5, 15, 1, 1);

            vector.Print();
            vector1.Print();

            vector.ConsecutiveOP_IP(vector1, Operations.multiplication);
            vector.Print();

            //int[] results = new int[200];

            //float previousresult = 0f;



            //for (int j = 1; j < 1000; j++)
            //{
            //    float actresult = (((j*50) * ((j*100)+1))) - (((j-1)*50) * (((j-1)*100)+1));

            //    Parallel.For(0, 100, i =>
            //    {
            //        int id = 0;

            //        Vector vector = Vector.Arange(gpu, 0, 10, 2, 2);

            //        if (vector.Id != null)
            //        {
            //            id = (int)vector.Id;
            //        }
            //        else
            //        {
            //            id = 0;
            //        }

            //        results[i] = id;

            //        vector.Dispose();
            //    });


            //    if (results.Sum() == actresult)
            //    {
            //        Console.WriteLine("Passed");
            //    }
            //    else
            //    {

            //        Console.WriteLine($"Failed : Recieved {results.Sum()} | Expected : {actresult}");
            //    }


            //    previousresult = actresult;

            //}






        }






    }
}
