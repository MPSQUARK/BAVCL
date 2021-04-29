using ILGPU;
using ILGPU.Runtime;
using System;
using System.Linq;

namespace DataScience
{
    /// <summary>
    /// 
    /// </summary>
    public class Setup 
    { 
    

    
    }

    /// <summary>
    /// Class for 1D and 2D Vector support
    /// Float Precision
    /// </summary>
    public class Vector 
    {
        // VARIABLE BLOCK
        public float[] Value { get; set; }
        public int Columns { get; set; }

        // CONSTRUCTOR
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columns"></param>
        public Vector(float[] value, int columns=1)
        {

            this.Value = value;
            this.Columns = columns;

        }

        // FEATURES
        /* LOG :
         *      - Access Slice                          : NI
         *      - Access Value                          : IMPLEMENTED
         *      - Consecutive Operation                 : NI
         *      - Dot Product                           : NI
         *      - Fill                                  : WORKING
         *      - Scalar Operation                      : NI
         *      - Normalise                             : NI
        */

        // METHODS SECTION

        // CREATION
        #region
        /// <summary>
        /// Creates a UNIFORM Vector where all values are equal to Value
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Size"></param>
        /// <param name="Columns"></param>
        /// <returns></returns>
        public static Vector Fill(float Value, int Size, int Columns = 1)
        {
            return new Vector(Enumerable.Repeat(Value, Size).ToArray(), Columns);
        }
        /// <summary>
        /// Sets all values in THIS Vector to value, of a set size and columns
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Size"></param>
        /// <param name="Columns"></param>
        /// <param name="inplace"></param>
        public void Fill(float Value, int Size, int Columns = 1, bool inplace= true)
        {
            this.Value = Enumerable.Repeat(Value, Size).ToArray();
            this.Columns = Columns;
        }


        #endregion

        // MEMORY ACCESS
        #region
        /// <summary>
        /// Access 1 Value from 1D or 2D Vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static float AccessVal(Vector vector, int row, int col)
        {
            return vector.Value[row * vector.Columns + col];
        }
        /// <summary>
        /// Access 1 Value from this 1D or 2D Vector
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public float AccessVal(int row, int col)
        {
            return this.Value[row * this.Columns + col];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gpu"></param>
        /// <param name="vector"></param>
        /// <param name="row_col_index"></param>
        /// <param name="row_col"></param>
        /// <returns></returns>
        public static Vector AccessSlice(Accelerator gpu, Vector vector, int row_col_index, char row_col)
        {

            if (vector.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            int[] ChangeSelectLength;
            int OutPutVectorLength;

            switch (row_col)
            {
                case 'r':
                    //ChangeSelectLength = new int[5] { 0, 1, row_col_index, 0, vector.Columns };
                    //OutPutVectorLength = vector.Columns;
                    return AccessRow(vector, row_col_index);
                case 'c':
                    ChangeSelectLength = new int[5] { 1, 0, 0, row_col_index, vector.Columns };
                    OutPutVectorLength = vector.Value.Length / vector.Columns;
                    break;
                default:
                    throw new Exception("Invalid slice char selector, choose 'r' for row or 'c' for column");
            }

            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernal);

            var buffer = gpu.Allocate<float>(OutPutVectorLength);
            var buffer2 = gpu.Allocate<float>(vector.Value.Length);
            var buffer3 = gpu.Allocate<int>(5);

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);
            buffer3.MemSetToZero(Stream);

            buffer2.CopyFrom(Stream, vector.Value, 0, 0, vector.Value.Length);
            buffer3.CopyFrom(Stream, ChangeSelectLength, 0, 0, ChangeSelectLength.Length);

            kernelWithStream(Stream, OutPutVectorLength, buffer.View, buffer2.View, buffer3.View);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            Stream.Dispose();

            return new Vector(Output);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gpu"></param>
        /// <param name="row_col_index"></param>
        /// <param name="row_col"></param>
        /// <returns></returns>
        public Vector AccessSlice(Accelerator gpu, int row_col_index, char row_col)
        {

            if (this.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            int[] ChangeSelectLength;
            int OutPutVectorLength;

            switch (row_col)
            {
                case 'r':
                    //ChangeSelectLength = new int[5] { 0, 1, row_col_index, 0, vector.Columns };
                    //OutPutVectorLength = vector.Columns;
                    return AccessRow(this, row_col_index);
                case 'c':
                    ChangeSelectLength = new int[5] { 1, 0, 0, row_col_index, this.Columns };
                    OutPutVectorLength = this.Value.Length / this.Columns;
                    break;
                default:
                    throw new Exception("Invalid slice char selector, choose 'r' for row or 'c' for column");
            }

            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernal);

            var buffer = gpu.Allocate<float>(OutPutVectorLength);
            var buffer2 = gpu.Allocate<float>(this.Value.Length);
            var buffer3 = gpu.Allocate<int>(5);

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);
            buffer3.MemSetToZero(Stream);

            buffer2.CopyFrom(Stream, this.Value, 0, 0, this.Value.Length);
            buffer3.CopyFrom(Stream, ChangeSelectLength, 0, 0, ChangeSelectLength.Length);

            kernelWithStream(Stream, OutPutVectorLength, buffer.View, buffer2.View, buffer3.View);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            Stream.Dispose();

            return new Vector(Output);
        }
        // KERNEL
        static void AccessSliceKernal(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
        {
            OutPut[index] = Input[
                index * ChangeSelectLength[0] * ChangeSelectLength[4] + // iRcL
                index * ChangeSelectLength[1] +                         // iCc
                ChangeSelectLength[2] * ChangeSelectLength[4] +         // RsL
                ChangeSelectLength[3]];                                 // Cs
        }
        public static Vector AccessRow(Vector vector, int row)
        {
            return new Vector(vector.Value[(row * vector.Columns)..((row + 1) * vector.Columns)], 1);
        }


        #endregion

        // FUNCTIONS



    }


    /// <summary>
    /// 
    /// </summary>
    public class Vector3
    {



    }



}
