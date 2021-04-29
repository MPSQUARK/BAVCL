using System;
using DataScience;
using ILGPU;
using ILGPU.Runtime;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Context context = new Context();
            Accelerator gpu;

            gpu = Setup.GetGPU(context);

            // SAMPLE AND TEST CODE

            Vector vectorA = Vector.Linspace(-10, 10, 5);
            Vector vectorB = Vector.Arange(-10, 10, 5);

            vectorA.Print();
            vectorB.Print();

            vectorA._ConsecutiveOP(gpu, 5f);
            vectorA.Print();

        }
    }
}
