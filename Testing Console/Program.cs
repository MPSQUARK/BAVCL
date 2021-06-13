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

            Vector vector = Vector.Arange(gpu, 20, 0, -0.5f, 5);
            Vector vector1 = new Vector(gpu, new float[]{1f, 2f, 3f, 4f, 5f, 6f, 7f}, 3);



        }






    }
}
