using DataScience;
using System;
using System.Diagnostics;
using DataScience.Geometric;
using DataScience.Utility;


namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU(memorycap:0.94f);
            Random rnd = new Random(522);

            Vector vec = Vector.Arange(gpu,0, 20000000, 1, 4);

            //vec.Print();

            float[] time = new float[2];
            int reps = 100;


            Vector vec2 = Vector.Arange(gpu, 0, 12, 1, 4);



            //Console.WriteLine(Util.ToString(Vector.GetColumnAsArray(vec2, 1)));
            //Console.WriteLine(Util.ToString(vec2.GetColumnAsArray(1)));


            //Stopwatch sw = new();

            //for (int i = 0; i < reps; i++)
            //{
            //    Vector.GetColumnAsArray(vec, 1);
            //}

            //gpu.DeCacheLRU(gpu.MaxMemory);
            //vec.Cache();

            //sw.Start();
            //for (int i = 0; i < reps; i++)
            //{
            //    Vector.GetColumnAsArray(vec, 1);
            //}
            //time[0] = (float)sw.ElapsedMilliseconds / (float)reps;




            //for (int i = 0; i < reps; i++)
            //{
            //    vec.GetColumnAsArray(1);
            //}

            //gpu.DeCacheLRU(gpu.MaxMemory);
            //vec.Cache();

            //sw.Restart();
            //for (int i = 0; i < reps; i++)
            //{
            //    vec.GetColumnAsArray(1);
            //}
            //time[1] = (float)sw.ElapsedMilliseconds / (float)reps;



            //Console.WriteLine(Util.ToString(time, decimalplaces:2));

        }






    }
}
