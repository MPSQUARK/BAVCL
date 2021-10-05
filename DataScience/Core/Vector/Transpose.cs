using ILGPU.Runtime;
using System;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Transpose(Vector vector)
        {
            if (vector.Columns == 1 || vector.Columns >= vector.Value.Length) { throw new Exception("Cannot transpose 1D Vector"); }

            // Ensure there is enough space for all the data
            long size = vector._memorySize << 1;

            // Prevent from decache
            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new Vector(vector.gpu, new float[vector.Value.Length], vector.RowCount());

            // Prevent from decache
            Output.IncrementLiveCount();

            vector.gpu.DeCacheLRU(size);

            MemoryBuffer<float> buffer = Output.GetBuffer(); // Output
            MemoryBuffer<float> buffer2 = vector.GetBuffer(); // Input

            vector.gpu.transposekernel(vector.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, vector.Columns);

            vector.gpu.accelerator.Synchronize();

            buffer.CopyTo(Output.Value, 0, 0, Output.Value.Length);

            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }
        public void Transpose_IP()
        {
            throw new Exception("WIP : Please use the static version of this function under Vector.Transpose");

#pragma warning disable CS0162 // Unreachable code detected
            Vector vector = Transpose(this);
#pragma warning restore CS0162 // Unreachable code detected
            this.TryDeCache();

            this.Value = vector.Value[..];
            this._id = vector._id;
            this.Columns = vector.Columns;

            return;
        }

    }
}
