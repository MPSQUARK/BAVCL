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

            int[] results = new int[200];

            float previousresult = 0f;

            int id = 0;

            for (int j = 1; j < 1000; j++)
            {
                float actresult = (((j*50) * ((j*100)+1))) - (((j-1)*50) * (((j-1)*100)+1));

                Parallel.For(0, 100, i =>
                {
                    Vector vector = Vector.Arange(gpu, 0, 10, 2, 2);

                    if (vector.Id != null)
                    {
                        id = (int)vector.Id;
                    }
                    else
                    {
                        id = 0;
                    }

                    results[i] = id;

                    //vector.Dispose();
                });

                //for (int i = 0; i < 100; i++)
                //{
                //    Vector vector = Vector.Arange(gpu, 0, 10, 2, 2);

                //    if (vector.Id != null)
                //    {
                //        id = (float)vector.Id;
                //    }
                //    else
                //    {
                //        id = 0;
                //    }

                //    results[i] = id;

                //    vector.Dispose();
                //}


                //Results.Print();

                if (results.Sum() == actresult)
                {
                    //Console.WriteLine("Passed");
                }
                else
                {
                    Console.WriteLine($"Failed : Recieved {results.Sum()} | Expected : {actresult}");
                }


                previousresult = actresult;

            }

            

            


        }






    }
}
