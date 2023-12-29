using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL
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
            return vectorA.Copy().Concat_IP(vectorB, axis, warp);
        }
        public Vector Concat_IP(Vector vector, char axis = 'r', bool warp = false)
        {
            if (axis == 'r')
            {
                this.Append_IP(vector);
                return this;
            }

            // IF Concat in COLUMN mode

            // IF 2D
            if (Columns > 1 && vector.Columns > 1)
            {
                if ((Rows != vector.Rows) && (Rows != vector.Columns))
                {
                    throw new Exception(
                        $"Vectors CANNOT be appended. " +
                        $"This Vector has the shape ({this.Rows},{this.Columns}). " +
                        $"The 2D Vector being appended has the shape ({vector.Rows},{vector.Columns})");
                }

                if (Rows == vector.Columns)
                {
                    if (!warp)
                    {
                        vector.Transpose_IP();
                    }

                    if (warp && (vector.Length % Rows == 0))
                    {
                        vector.Columns = vector.Value.Length / Rows;
                    }

                }

            }
            // IF 1D
            if (vector.Columns == 1)
            {

                if (vector.Value.Length % Rows != 0)
                {
                    throw new Exception($"Vectors CANNOT be appended. " +
                        $"This array has shape ({Rows},{Columns}), 1D vector being appended has {vector.Length} Length");
                }

                vector.Columns = vector.Value.Length / this.Rows;

            }

            Vector Output = new(gpu, vector.Length + Length);

            IncrementLiveCount();
            vector.IncrementLiveCount();
            Output.IncrementLiveCount();

            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = Output.GetBuffer(),        // Output
                buffer2 = GetBuffer(),              // Input
                buffer3 = vector.GetBuffer();       // Input

            gpu.appendKernel(gpu.accelerator.DefaultStream, this.Rows, buffer.View, buffer2.View, buffer3.View, this.Columns, vector.Columns);

            gpu.accelerator.Synchronize();

            DecrementLiveCount();
            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            this.Columns += vector.Columns;

            return TransferBuffer(Output);
        }

    }


}
