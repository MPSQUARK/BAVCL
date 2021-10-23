using DataScience;
using DataScience.Core;
using System;
using System.Diagnostics;
using DataScience.Geometric;
using DataScience.Utility;
using System.Linq;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU(memorycap:0.94f);
            Random rnd = new Random(522);


            string words =
                "something hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials hello me three five forty marcel materials" +
                "hello me three five forty marcel materials hello me three five forty marcel materials";
            
            VectorStr vector = new VectorStr(words.Split(" "));

            Console.WriteLine($"Longest Word : {vector.Max()}");
            Console.WriteLine($"Length of Array : {vector.Value.Length}");

            float[] time = new float[3];
            int reps = 100000;
            Stopwatch sw = new();


            for (int i = 0; i < reps; i++)
            {
                vector.Contains("something");
            }

            sw.Start();
            for (int i = 0; i < reps; i++)
            {
                vector.Contains("something");
            }
            time[0] = (float)sw.ElapsedMilliseconds;


            for (int i = 0; i < reps; i++)
            {
                vector.Contains2("something");
            }

            sw.Restart();
            for (int i = 0; i < reps; i++)
            {
                vector.Contains2("something");
            }
            time[1] = (float)sw.ElapsedMilliseconds;
            sw.Reset();


            for (int i = 0; i < reps; i++)
            {
                vector.Contains3("something");
            }

            sw.Restart();
            for (int i = 0; i < reps; i++)
            {
                vector.Contains3("something");
            }
            time[2] = (float)sw.ElapsedMilliseconds;
            sw.Reset();


            Console.WriteLine(Util.ToString(time));

        }






    }
}
