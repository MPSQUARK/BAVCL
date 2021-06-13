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

            Vector vector = Vector.Arange(gpu, 0, 20, 0.5f, 5);
            vector.Print();

            vector.AccessColumn(2).Print();
            vector.AccessRow(3).Print();

        }






    }
}
