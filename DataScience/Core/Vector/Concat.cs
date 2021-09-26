

using System;

namespace DataScience
{
    public partial class Vector
    {
        /// <summary>
        /// Concatinates VectorB onto the end of VectorA.
        /// Preserves the value of Columns of VectorA.
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        public static Vector Concat(Vector vectorA, Vector vectorB, char axis = 'r', bool warp = false)
        {
            Vector vector = vectorA.Copy();
            vector.Concat_IP(vectorB, axis, warp);
            return vector;
        }
        public void Concat_IP(Vector vector, char axis = 'r', bool warp = false)
        {
            if (axis == 'r')
            {
                this.Append_IP(vector);
                return;
            }

            // IF Concat in COLUMN mode

            // IF 2D
            if (this.Columns > 1 && vector.Columns > 1)
            {
                if ((this.RowCount() != vector.RowCount()) && (this.RowCount() != vector.Columns))
                {
                    throw new Exception(
                        $"Vectors CANNOT be appended. " +
                        $"This Vector has the shape ({this.RowCount()},{this.Columns}). " +
                        $"The 2D Vector being appended has the shape ({vector.RowCount()},{vector.Columns})");
                }

                if (this.RowCount() == vector.Columns)
                {
                    if (!warp)
                    {
                        vector.Transpose_IP();
                    }

                    if (warp && (vector.Length() % this.RowCount() == 0))
                    {
                        vector.Columns = vector.Value.Length / this.RowCount();
                    }

                }

            }
            // IF 1D
            if (vector.Columns == 1)
            {

                if (vector.Value.Length % this.RowCount() != 0)
                {
                    throw new Exception($"Vectors CANNOT be appended. " +
                        $"This array has shape ({this.RowCount()},{this.Columns}), 1D vector being appended has {vector.Length()} Length");
                }

                vector.Columns = vector.Value.Length / this.RowCount();

            }


            long size = (vector.MemorySize() << 1) + (this.MemorySize() << 1);

            Vector Output = new Vector(this.gpu, new float[vector.Value.Length + this.Value.Length]);

            uint[] Flags = new uint[] { this.Id, vector.Id, Output.Id };
            this.gpu.AddFlags(Flags);
            this.gpu.DeCacheLRU(size, true);

            var buffer = Output.GetBuffer();                                    // Output
            var buffer2 = this.GetBuffer();                                     // Input
            var buffer3 = vector.GetBuffer();                                   // Input

            gpu.appendKernel(gpu.accelerator.DefaultStream, this.RowCount(), buffer.View, buffer2.View, buffer3.View, this.Columns, vector.Columns);

            gpu.accelerator.Synchronize();

            this.gpu.RemoveFlags(Flags);

            this.Columns += vector.Columns;
            this.Value = buffer.GetAsArray();

            this.Dispose();

            this.Id = Output.Id;
            

        }

    }


}
