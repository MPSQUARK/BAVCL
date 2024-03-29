﻿using ILGPU;
using ILGPU.Runtime;

namespace BAVCL
{
    public partial class Vector
    {

        public static Vector GetColumnAsVector(Vector vector, int column)
        {
            // Get config data needed
            int[] select = new int[2] { column, vector.Columns };

            // Secure the Input
            vector.IncrementLiveCount();

            // Make Output Vector
            Vector Output = new(vector.gpu, vector.RowCount());
            
            // Secure the Output
            Output.IncrementLiveCount();

            // Get Memory buffer Data
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vector.GetBuffer();       // Input

            // Allocate config Data onto GPU
            MemoryBuffer1D<int, Stride1D.Dense>
                buffer3 = vector.gpu.accelerator.Allocate1D(select);      // Config

            // RUN
            vector.gpu.getSliceKernel(vector.gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View);

            // SYNC
            vector.gpu.accelerator.Synchronize();

            // Dispose of Config
            buffer3.Dispose();

            // Remove Security
            Output.DecrementLiveCount();
            vector.DecrementLiveCount();

            return Output;
        }

        public Vector GetColumnAsVector(int column)
        {
            // Get config data needed
            int[] select = new int[2] { column, Columns };

            // Secure the Input & Output
            IncrementLiveCount();

            // Make Output Vector
            Vector Output = new(gpu, RowCount());

            Output.IncrementLiveCount();

            // Get Memory buffer Data
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = GetBuffer();              // Input

            // Allocate config Data onto GPU
            MemoryBuffer1D<int, Stride1D.Dense>
                buffer3 = gpu.accelerator.Allocate1D(select);     // Config

            // RUN
            gpu.getSliceKernel(gpu.accelerator.DefaultStream, RowCount(), buffer.View, buffer2.View, buffer3.View);

            // SYNC
            gpu.accelerator.Synchronize();

            // Dispose of Config
            buffer3.Dispose();

            // Remove Security
            DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }

        public static float[] GetColumnAsArray(Vector vector, int column)
        {
            Vector output = vector.GetColumnAsVector(column);
            output.DeCache();
            return output.Value;
        }

        public float[] GetColumnAsArray(int column)
        {
            Vector output = GetColumnAsVector(column);
            output.DeCache();
            return output.Value;
        }

    }


}
