using System;
using DataScience;
using ILGPU;
using ILGPU.Runtime;
using System.Linq;

using System.Diagnostics;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();

            // SAMPLE AND TEST CODE

            Vector vec = Vector.Linspace(gpu, 0, 25, 25, 5);
            Vector vec2 = Vector.Fill(gpu, 8, 10, 2);
            Vector vec3 = Vector.Fill(gpu, 8, 10, 2);

            vec.Print();
            vec2.Print();
            vec._Append(vec2,'c');
            vec._Prepend(vec3, 'c');
            vec.Print();

            //Vector.Append(vec, vec3, 'c').Print();

            //Vector vectorA = Vector.Linspace(gpu, -100, -1, 100);
            //Vector vectorB = Vector.Linspace(gpu, -100, 100, 200);
            //Vector vectorC = Vector.Linspace(gpu, 100000000, 250000000, 100);
            //Vector vectorD = Vector.Linspace(gpu, -100000000, -250000000, 25);
            //Vector vectorE = Vector.Linspace(gpu, 100, -100, 201);
            //Vector vectorF = Vector.Linspace(gpu, 642.21f, 8412.1f, 55);

            //vectorB = Vector.Arange(gpu, -10, 10, 1);
            //vectorB.Print();


            //Vector vectorD = Vector.Linspace(-5, 5, 10);
            //vectorA.Columns = 10;
            //vectorB.Columns = 10;
            //vectorC.Columns = 5;
            //vectorD.Columns = 5;
            //vectorE.Columns = 10;
            //vectorF.Columns = 11;

            //vectorD.Columns = 5;

            //vectorB.Print();

            //Console.Write(vectorB.ToCSV());
            //Console.WriteLine();





        }
    }
}
