using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL
{
    public partial class Vector
    {
        public static Vector Transpose(Vector vector)
        {
            if (vector.Columns == 1 || vector.Columns >= vector.Length) { throw new Exception("Cannot transpose 1D Vector"); }

            // Prevent from decache
            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new(vector.Gpu, vector.Length, vector.RowCount());

            // Prevent from decache
            Output.IncrementLiveCount();

            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = Output.GetBuffer(), // Output
                buffer2 = vector.GetBuffer(); // Input

            vector.Gpu.transposekernel(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, vector.Columns);

            vector.Gpu.accelerator.Synchronize();

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
