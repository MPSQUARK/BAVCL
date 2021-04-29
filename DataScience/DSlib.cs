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
         *      - Consecutive OP                        : NI
         *      - Dot Product                           : NI
         *      - Fill                                  : WORKING
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startval"></param>
        /// <param name="endval"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public static Vector Linspace(float startval, float endval, int steps)
        {
            float interval = (Math.Abs(startval) + Math.Abs(endval)) / (steps - 1);
            return new Vector((from val in Enumerable.Range(0, steps)
                    select startval + (val * interval)).ToArray(),1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startval"></param>
        /// <param name="endval"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Vector Arange(float startval, float endval, float interval)
        {
            int steps = (int)((Math.Abs(startval) + Math.Abs(endval)) / interval);
            return new Vector((from val in Enumerable.Range(0, steps)
                               select startval + (val * interval)).ToArray(), 1);
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
        public float _AccessVal(int row, int col)
        {
            return this.Value[row * this.Columns + col];
        }

        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
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
        /// Access a specific slice of either a column 'c' or row 'r' of this vector
        /// </summary>
        /// <param name="gpu"></param>
        /// <param name="row_col_index"></param>
        /// <param name="row_col"></param>
        /// <returns></returns>
        public Vector _AccessSlice(Accelerator gpu, int row_col_index, char row_col)
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
                    return this._AccessRow(this, row_col_index);
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
        /// <summary>
        /// Access a specified row of a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Vector AccessRow(Vector vector, int row)
        {
            return new Vector(vector.Value[(row * vector.Columns)..((row + 1) * vector.Columns)], 1);
        }
        /// <summary>
        /// Access a specific row of this Vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public Vector _AccessRow(Vector vector, int row)
        {
            return new Vector(vector.Value[(row * vector.Columns)..((row + 1) * vector.Columns)], 1);
        }

        #endregion

        // FUNCTIONS
        public static Vector ConsecutiveOP(Accelerator gpu, Vector vectorA, Vector vectorB, string operation = "*")
        {
            if (vectorA.Value.Length != vectorB.Value.Length)
            {
                throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
            }

            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveProductKernal);

            var buffer = gpu.Allocate<float>(vectorA.Value.Length); // Input
            var buffer2 = gpu.Allocate<float>(vectorA.Value.Length); // Input
            var buffer3 = gpu.Allocate<float>(vectorA.Value.Length); // Output

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);
            buffer3.MemSetToZero(Stream);

            buffer.CopyFrom(Stream, vectorA.Value, 0, 0, vectorA.Value.Length);
            buffer2.CopyFrom(Stream, vectorB.Value, 0, 0, vectorB.Value.Length);

            switch (operation)
            {
                case "*":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveProductKernal);
                    break;
                case "+":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveAdditionKernal);
                    break;
                case "-":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveSubtractKernal);
                    break;
                case "/":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveDivisionKernal);
                    break;
            }

            kernelWithStream(Stream, buffer.Length, buffer.View, buffer2.View, buffer3.View);

            Stream.Synchronize();

            float[] Output = buffer3.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            Stream.Dispose();

            return new Vector(Output);
        }
        public Vector _ConsecutiveOP(Accelerator gpu, Vector vectorB, string operation = "*")
        {
            if (this.Value.Length != vectorB.Value.Length)
            {
                throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
            }

            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveProductKernal);

            var buffer = gpu.Allocate<float>(this.Value.Length); // Input
            var buffer2 = gpu.Allocate<float>(this.Value.Length); // Input
            var buffer3 = gpu.Allocate<float>(this.Value.Length); // Output

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);
            buffer3.MemSetToZero(Stream);

            buffer.CopyFrom(Stream, this.Value, 0, 0, this.Value.Length);
            buffer2.CopyFrom(Stream, vectorB.Value, 0, 0, vectorB.Value.Length);

            switch (operation)
            {
                case "*":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveProductKernal);
                    break;
                case "+":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveAdditionKernal);
                    break;
                case "-":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveSubtractKernal);
                    break;
                case "/":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(ConsecutiveDivisionKernal);
                    break;
            }

            kernelWithStream(Stream, buffer.Length, buffer.View, buffer2.View, buffer3.View);

            Stream.Synchronize();

            float[] Output = buffer3.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            Stream.Dispose();

            return new Vector(Output);
        }
        // KERNEL
        static void ConsecutiveProductKernal(Index1 index, ArrayView<float> InputA, ArrayView<float> InputB, ArrayView<float> OutPut)
        {

            OutPut[index] = InputA[index] * InputB[index];

        }
        static void ConsecutiveAdditionKernal(Index1 index, ArrayView<float> InputA, ArrayView<float> InputB, ArrayView<float> OutPut)
        {

            OutPut[index] = InputA[index] + InputB[index];

        }
        static void ConsecutiveSubtractKernal(Index1 index, ArrayView<float> InputA, ArrayView<float> InputB, ArrayView<float> OutPut)
        {

            OutPut[index] = InputA[index] - InputB[index];

        }
        static void ConsecutiveDivisionKernal(Index1 index, ArrayView<float> InputA, ArrayView<float> InputB, ArrayView<float> OutPut)
        {

            OutPut[index] = InputA[index] / InputB[index];

        }
        public static Vector ConsecutiveOP(Accelerator gpu, Vector vector, float scalar, string operation = "*")
        {
            AcceleratorStream Stream = gpu.CreateStream();

            var buffer = gpu.Allocate<float>(vector.Value.Length);
            var buffer2 = gpu.Allocate<float>(vector.Value.Length);

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);

            buffer2.CopyFrom(Stream, vector.Value, 0, 0, vector.Value.Length);

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarProductKernal);

            switch (operation)
            {
                case "*":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarProductKernal);
                    break;
                case "/":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarDivideKernal);
                    break;
                case "+":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarSumKernal);
                    break;
                case "^*":  // flip the Vector e.g. 1/Vector then multiply by Scalar
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarProductInvVecKernal);
                    break;
            }

            kernelWithStream(Stream, buffer.Length, buffer.View, buffer2.View, scalar);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();

            Stream.Dispose();

            return new Vector(Output);
        }
        public Vector _ConsecutiveOP(Accelerator gpu, float scalar, string operation = "*")
        {
            AcceleratorStream Stream = gpu.CreateStream();

            var buffer = gpu.Allocate<float>(this.Value.Length);
            var buffer2 = gpu.Allocate<float>(this.Value.Length);

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);

            buffer2.CopyFrom(Stream, this.Value, 0, 0, this.Value.Length);

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarProductKernal);

            switch (operation)
            {
                case "*":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarProductKernal);
                    break;
                case "/":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarDivideKernal);
                    break;
                case "+":
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarSumKernal);
                    break;
                case "^*":  // flip the Vector e.g. 1/Vector then multiply by Scalar
                    kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float>(ScalarProductInvVecKernal);
                    break;
            }

            kernelWithStream(Stream, buffer.Length, buffer.View, buffer2.View, scalar);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();

            Stream.Dispose();

            return new Vector(Output);
        }
        // KERNELS
        static void ScalarProductKernal(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar)
        {
            OutPut[index] = Input[index] * Scalar;
        }
        static void ScalarDivideKernal(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar)
        {
            OutPut[index] = Input[index] / Scalar;
        }
        static void ScalarSumKernal(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar)
        {
            OutPut[index] = Input[index] + Scalar;
        }
        static void ScalarProductInvVecKernal(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar)
        {
            OutPut[index] = Scalar / Input[index];
        }





    }


    /// <summary>
    /// 
    /// </summary>
    public class Vector3
    {



    }



}
