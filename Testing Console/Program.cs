using DataScience;
using System;
using System.Diagnostics;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();

            // SAMPLE AND TEST CODE

            
            Vertex vertB = new Vertex(8f, 1f, 9f);
            vertB.Print();
            Vertex vertex1 = Vertex.Reinhard(vertB);
            vertB.Print();
            vertex1.Print();



            //Vertex vertResOriginal = Vertex.RefractOrig(vertA, vertB, 4f);
            //Vertex vertResModed = Vertex.Refract(vertA, vertB, 4f);

            //Console.WriteLine($"Testing Check function : {Vertex.FORWARD().Equals(Vertex.FORWARD())}");
            //Console.WriteLine($"Testing Refract function : {vertResOriginal.Equals(vertResModed)}");

            //Console.WriteLine($"Original x : {vertResOriginal.x}, \nModded x : {vertResModed.x}");
            //vertResOriginal.Print();
            //vertResModed.Print();

            float time = 0f;

            for (int i = 0; i < 10000; i++)
            {
                Vertex vertA = new Vertex(4f, 7f, 1f);
                Vertex vertex = Vertex.Reinhard(vertA);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1_000_000; i++)
            {
                Vertex vertA = new Vertex(4f, 7f, 1f);
                Vertex vertex = Vertex.Reinhard(vertA);

                //vertex.Print();
            }
            time = sw.ElapsedMilliseconds;
            sw.Stop();

            Console.WriteLine($"Time taken for Original Reinhard : {time} ms");


            for (int i = 0; i < 10000; i++)
            {
                Vertex vertA = new Vertex(4f, 7f, 1f);
                Vertex copyVertA = new Vertex(vertA.x, vertA.y, vertA.z);
                copyVertA._Reinhard();
            }

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 1_000_000; i++)
            {
                Vertex vertA = new Vertex(4f, 7f, 1f);
                Vertex copyVertA = new Vertex(vertA.x, vertA.y, vertA.z);
                copyVertA._Reinhard();

                //copyVertA.Print();
            }
            time = sw.ElapsedMilliseconds;
            sw.Stop();

            Console.WriteLine($"Time taken for modified Reinhard : {time} ms");


            //vecR.Print();


            //Vector3 geoVec3 = new Vector3(gpu, new float[] { 88, 53, 5, 9 });

            //Vector vec5 = Vector.Linspace(gpu, 25, 1, 25, 5);
            //Vector vec2 = Vector.Linspace(gpu, -25, 25, 51, 5);
            //Vector vec3 = Vector.Linspace(gpu, -1, -25, 25, 5);
            //Vector vec4 = Vector.Linspace(gpu, -25, -1, 25, 5);


            //vec5.Print();
            //vec2.Print();
            //vec3.Print();
            //vec4.Print();

            //Vector vec2 = Vector.Fill(gpu, 0, 25, 5);

            //Vector vec = new Vector(gpu, new float[] { 1.1f, 1.2f, 1.3f, 1.4f, 1.5f });
            //Vector vec2 = new Vector(gpu, new float[] { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f });

            //vec.Print();
            //vec2.Print();

            //Console.WriteLine(Vector.DotProduct(vec, vec2));
            //vec._ReverseX();
            //vec.Print();

            //vec._Reciprocal();

            //vec = Vector.Reciprocal(vec);
            //vec.Print();


            //vec.Print();
            //vec2.Print();
            //Vector vecR = Vector.ConsecutiveOP(vec, vec2, "*");
            //vecR.Print();

            //vecR._Nan_to_num(55f);
            //vecR.Print();


            //Console.WriteLine();
            //Vector vec2 = Vector.Fill(gpu, 8, 10, 2);
            //Vector vec3 = Vector.Fill(gpu, 8, 10, 2);

            //vec.Print();
            //vec2.Print();
            //vec._Append(vec2,'c');
            //vec._Prepend(vec3, 'c');
            //vec.Print();

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

            SubtractionTest(gpu, 10_000_000);



        }

        public static void SubtractionTest(GPU gpu, int size)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Vector a = Vector.Linspace(gpu, 0, 1, size, 1);
            Vector b = Vector.Linspace(gpu, 1, 0, size, 1);

            timer.Stop();
            TimeSpan constructionTime = timer.Elapsed;
            timer.Restart();

            a._ConsecutiveOP(10, Operations.multiplication);
            b._ConsecutiveOP(10, Operations.multiplication);

            a._ConsecutiveOP(b, Operations.subtraction);

            timer.Stop();
            TimeSpan calculationTime = timer.Elapsed;

            if (a.Value[0] == 0)
            {
                Console.WriteLine("SubtractionTest Passed");
            }
            else
            {
                Console.WriteLine("SubtractionTest Failed");
            }

            Console.WriteLine("Construction Time: " + constructionTime.TotalMilliseconds + " ms");
            Console.WriteLine("Calculation Time: " + calculationTime.TotalMilliseconds + " ms");
        }
    }
}
