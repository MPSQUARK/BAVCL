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

            // Ensure there is enough space for all the data
            long size = vector._memorySize << 1;

            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new Vector(vector.gpu, new float[vector.Value.Length - 1], vector.Columns);

            Output.IncrementLiveCount();

            vector.gpu.DeCacheLRU(size);

            MemoryBuffer<float> buffer = Output.GetBuffer(); // Output
            MemoryBuffer<float> buffer2 = vector.GetBuffer(); // Input

            vector.gpu.diffKernel(vector.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View);

            vector.gpu.accelerator.Synchronize();

            buffer.CopyTo(Output.Value, 0, 0, Output.Value.Length);

            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }

        public void Diff_IP()
        {
            Vector Output = Vector.Diff(this);

            this.TryDeCache();
            this.Value = Output.Value[..];
            this._id = Output._id;

            return;
        }

    }




}
