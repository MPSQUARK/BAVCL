using ILGPU.Runtime;
using System;

namespace DataScience.Geometric
{
    public partial class Vector3
    {
        public static Vector3 Cross(Vector3 VectorA, Vector3 VectorB)
        {
            if (VectorA.Length != VectorB.Length) { throw new Exception($"Cannot Cross Product two Vector3's together of different lengths. {VectorA.Length} != {VectorB.Length}"); }

            // Cache the GPU
            GPU gpu = VectorA.gpu;

            Vector3 Output = new Vector3(gpu, new float[VectorA._length], true);

            VectorA.IncrementLiveCount();
            VectorB.IncrementLiveCount();
            Output.IncrementLiveCount();

            MemoryBuffer<float>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = VectorA.GetBuffer(),      // Input
                buffer3 = VectorB.GetBuffer();      // Input

            gpu.crossKernel(gpu.accelerator.DefaultStream, VectorA._length / 3, buffer.View, buffer2.View, buffer3.View);

            gpu.accelerator.Synchronize();

            VectorA.DecrementLiveCount();
            VectorB.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;

        }
    }
}
