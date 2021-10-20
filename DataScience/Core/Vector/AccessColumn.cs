using ILGPU.Runtime;

namespace DataScience
{
    public partial class Vector
    {

        public static Vector AccessColumn(Vector vector, int column)
        {
            // Get config data needed
            int[] select = new int[2] { column, vector.Columns };

            // Secure the Input
            vector.IncrementLiveCount();

            // Make Output Vector
            Vector Output = new Vector(vector.gpu, new float[vector.RowCount()]);
            
            // Secure the Output
            Output.IncrementLiveCount();

            // Get Memory buffer Data
            MemoryBuffer<float> 
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vector.GetBuffer();       // Input

            // Allocate config Data onto GPU
            MemoryBuffer<int> 
                buffer3 = vector.gpu.accelerator.Allocate<int>(2);      // Config
            buffer3.CopyFrom(select, 0, 0, select.Length);

            // RUN
            vector.gpu.accessSliceKernel(vector.gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View);

            // SYNC
            vector.gpu.accelerator.Synchronize();

            // Dispose of Config
            buffer3.Dispose();

            // Remove Security
            Output.DecrementLiveCount();
            vector.DecrementLiveCount();

            return Output;
        }

        public Vector AccessColumn(int column)
        {
            // Get config data needed
            int[] select = new int[2] { column, Columns };

            // Secure the Input & Output
            IncrementLiveCount();

            // Make Output Vector
            Vector Output = new Vector(gpu, new float[RowCount()]);

            Output.IncrementLiveCount();

            // Get Memory buffer Data
            MemoryBuffer<float> 
                buffer = Output.GetBuffer(),        // Output
                buffer2 = GetBuffer();              // Input

            // Allocate config Data onto GPU
            MemoryBuffer<int> 
                buffer3 = gpu.accelerator.Allocate<int>(2);     // Config
            buffer3.CopyFrom(select, 0, 0, select.Length);

            // RUN
            gpu.accessSliceKernel(gpu.accelerator.DefaultStream, RowCount(), buffer.View, buffer2.View, buffer3.View);

            // SYNC
            gpu.accelerator.Synchronize();

            // Dispose of Config
            buffer3.Dispose();

            // Remove Security
            DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }



    }


}
