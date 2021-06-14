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

            Vector vector = Vector.Arange(gpu, 10, -10, -0.5f, 5);

            vector.Print();

            Vector vec = Vector.Abs(vector);

            vector.Print();
            vec.Print();


        }






    }
}
