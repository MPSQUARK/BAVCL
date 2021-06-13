using DataScience;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using DataScience.IO;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();

            // SAMPLE AND TEST CODE

            Vector3 vector = new Vector3(gpu, new float[] { 1, 2, 3 });
            List<Vertex> vert =  new List<Vertex> { new Vertex(-8f, 5.4f, 12f), new Vertex(-99,-88,-77) };

            float time = 0f;

            vector.Print();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Vector3 vector3 = Vector3.AppendVert(vector, vert);
            time = sw.ElapsedMilliseconds;
            sw.Stop();
            sw.Reset();


            Console.WriteLine($"time taken for new Append : {time} ms");

            vector.Print();
            vector3.Print();

            
            sw.Start();
            vector.AppendVert_IP(vert);
            time = sw.ElapsedMilliseconds;
            sw.Stop();
            sw.Reset();

            Console.WriteLine($"time taken for IP Append : {time} ms");

            vector.Print();
            vector3.Print();

        }






    }
}
