using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL
{
    public partial class Vector
    {
        public static Vector Transpose(Vector vector)
        {
            if (vector.Columns == 1 || vector.Columns >= vector._length) { throw new Exception("Cannot transpose 1D Vector"); }

            // Prevent from decache
            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new(vector.gpu, new float[vector._length], vector.RowCount());

            // Prevent from decache
            Output.IncrementLiveCount();

            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = Output.GetBuffer(), // Output
                buffer2 = vector.GetBuffer(); // Input

            vector.gpu.transposekernel(vector.gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, vector.Columns);

            vector.gpu.accelerator.Synchronize();

            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }
        public Vector Transpose_IP()
        {
            return TransferBuffer(Transpose(this), true);
        }

    }
}
