using ILGPU.Runtime;
using System.Collections.Generic;

namespace DataScience
{
    public partial class Vector
    {

        public static Vector AccessColumn(Vector vector, int column)
        {
            // Get config data needed
            int[] select = new int[2] { column, vector.Columns };

            // Make Output Vector
            Vector Output = new Vector(vector.gpu, new float[vector.RowCount()]);

            // Secure the Input & Output
            Output.IncrementLiveCount();
            vector.IncrementLiveCount();

            // Make space for data
            vector.gpu.DeCacheLRU((vector._memorySize + Output._memorySize + 8));

            // Get Memory buffer Data
            MemoryBuffer<float> buffer = Output.GetBuffer();                                            // Output
            MemoryBuffer<float> buffer2 = vector.GetBuffer();                                           // Input

            // Allocate config Data onto GPU
            MemoryBuffer<int> buffer3 = vector.gpu.accelerator.Allocate<int>(2);                        // Config
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
            int[] select = new int[2] { column, this.Columns };

            // Make Output Vector
            Vector Output = new Vector(this.gpu, new float[this.RowCount()]);

            // Secure the Input & Output
            this.IncrementLiveCount();
            Output.IncrementLiveCount();

            // Make space for data
            this.gpu.DeCacheLRU((this._memorySize + Output._memorySize + 8));

            // Get Memory buffer Data
            MemoryBuffer<float> buffer = Output.GetBuffer();                                        // Output
            MemoryBuffer<float> buffer2 = this.GetBuffer();                                         // Input

            // Allocate config Data onto GPU
            MemoryBuffer<int> buffer3 = this.gpu.accelerator.Allocate<int>(2);                      // Config
            buffer3.CopyFrom(select, 0, 0, select.Length);

            // RUN
            this.gpu.accessSliceKernel(this.gpu.accelerator.DefaultStream, this.RowCount(), buffer.View, buffer2.View, buffer3.View);

            // SYNC
            this.gpu.accelerator.Synchronize();

            // Dispose of Config
            buffer3.Dispose();

            // Remove Security
            this.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }



    }


}
