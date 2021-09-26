using System.Collections.Generic;

namespace DataScience
{
    public partial class Vector
    {

        public static Vector AccessColumn(Vector vector, int column)
        {
            int[] select = new int[2] { column, vector.Columns };

            Vector Output = new Vector(vector.gpu, new float[vector.RowCount()]);

            long size = vector.MemorySize() + Output.MemorySize() + 8;

            vector.gpu.DeCacheLRU(size, new HashSet<uint>() { Output.Id, vector.Id });

            var buffer = Output.GetBuffer();                                            // Output
            var buffer2 = vector.GetBuffer();                                           // Input

            var buffer3 = vector.gpu.accelerator.Allocate<int>(2);                      // Config
            buffer3.CopyFrom(select, 0, 0, select.Length);

            vector.gpu.accessSliceKernel(vector.gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View);

            vector.gpu.accelerator.Synchronize();

            Output.Value = buffer.GetAsArray();

            buffer3.Dispose();

            return Output;
        }
        public Vector AccessColumn(int column)
        {
            int[] select = new int[2] { column, this.Columns };

            Vector Output = new Vector(this.gpu, new float[this.RowCount()]);

            long size = this.MemorySize() + Output.MemorySize() + 8;

            this.gpu.DeCacheLRU(size, new HashSet<uint>() { Output.Id, this.Id });

            var buffer = Output.GetBuffer();                                        // Output
            var buffer2 = this.GetBuffer();                                         // Input
            var buffer3 = this.gpu.accelerator.Allocate<int>(2);                    // Config
            buffer3.CopyFrom(select, 0, 0, select.Length);

            this.gpu.accessSliceKernel(this.gpu.accelerator.DefaultStream, this.RowCount(), buffer.View, buffer2.View, buffer3.View);

            this.gpu.accelerator.Synchronize();

            Output.Value = buffer.GetAsArray();

            buffer3.Dispose();

            return Output;
        }



    }


}
