using System;
using DataScience.Utility;
using System.Text;
using ILGPU.Runtime;

namespace DataScience.Geometric
{

    public partial class Vector3
    {

        public static Vector3 OP(Vector3 vectorA, Vector3 vectorB, Operations operation)
        {
            GPU gpu = vectorA.gpu;

            vectorA.IncrementLiveCount();
            vectorB.IncrementLiveCount();

            // Make the Output Vector
            Vector3 Output = new Vector3(gpu, new float[vectorA._length]);
            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vectorA.GetBuffer(),      // Input
                buffer3 = vectorB.GetBuffer();      // Input

            // Run the kernel
            gpu.a_opFKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            gpu.accelerator.Synchronize();

            vectorA.DecrementLiveCount();
            vectorB.DecrementLiveCount();
            Output.DecrementLiveCount();

            // Return the result
            return Output;
        }
        public Vector3 OP(Vector3 vector, Operations operation)
        {
            GPU gpu = this.gpu;

            IncrementLiveCount();
            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector3 Output = new Vector3(gpu, new float[_length]);
            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = GetBuffer(),              // Input
                buffer3 = vector.GetBuffer();       // Input

            // Run the kernel
            gpu.a_opFKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            gpu.accelerator.Synchronize();

            DecrementLiveCount();
            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            // Return the result
            return Output;
        }


        public static Vector3 OP(Vector3 vector, float scalar, Operations operation)
        {
            GPU gpu = vector.gpu;

            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector3 Output = new Vector3(gpu, new float[vector._length]);

            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vector.GetBuffer();       // Input

            gpu.s_opFKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }
        public Vector3 OP(float scalar, Operations operation)
        {
            GPU gpu = this.gpu;

            IncrementLiveCount();

            // Make the Output Vector
            Vector3 Output = new Vector3(gpu, new float[_length]);

            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = this.GetBuffer();         // Input

            gpu.s_opFKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }





        public Vector3 OP_IP(Vector3 vector, Operations operation)
        {
            
            IncrementLiveCount();
            vector.IncrementLiveCount();
            
            // Check if the input & output are in Cache
            MemoryBuffer<float>
                buffer = GetBuffer(),               // IO
                buffer2 = vector.GetBuffer();       // Input

            // Run the kernel
            gpu.a_FloatOPKernelIP(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            gpu.accelerator.Synchronize();

            vector.DecrementLiveCount();
            DecrementLiveCount();

            return this;
        }
        public Vector3 OP_IP(float scalar, Operations operation)
        {
            IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = GetBuffer(); // IO

            gpu.s_FloatOPKernelIP(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, scalar, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            DecrementLiveCount();

            return this;
        }





    }


}
