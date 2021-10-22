using ILGPU.Runtime;
using System;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Diff(Vector vector)
        {
            if (vector.Columns > 1)
            {
                throw new Exception("Diff is for use with 1D Vectors ONLY");
            }

            GPU gpu = vector.gpu;

            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new Vector(gpu, new float[vector._length - 1], vector.Columns);

            Output.IncrementLiveCount();

            MemoryBuffer<float> 
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vector.GetBuffer();       // Input

            gpu.diffKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View);

            gpu.accelerator.Synchronize();

            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }

        public Vector Diff_IP()
        {
            return TransferBuffer(Vector.Diff(this));
        }

    }




}
