using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utility;

namespace DataScience
{

    /// <summary>
    /// Class for 1D and 2D Vector support
    /// Float Precision
    /// </summary>
    public class Vector
    {
        // VARIABLE BLOCK
        private GPU gpu;
        public float[] Value { get; set; }
        public int Columns { get; set; }

        // CONSTRUCTOR
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columns"></param>
        public Vector(GPU gpu, float[] value, int columns=1)
        {
            this.gpu = gpu;
            this.Value = value;
            this.Columns = columns;

        }


        // FEATURES
        /* LOG :
         *      - Access Slice                          : IMPLEMENTED
         *      - Access Value                          : IMPLEMENTED
         *      - Consecutive OP                        : IMPLEMENTED
         *      - Dot Product                           : IMPLEMENTED
         *      - Fill                                  : WORKING
         *      - Normalise                             : IMPLEMENTED
         *      - Linspace                              : IMPLEMENTED
         *      - Arange                                : IMPLEMENTED
         *      - Diff                                  : IMPLEMENTED
        */

        // METHODS SECTION

        // PRINT
        public static void Print(Vector vector)
        {
            Console.WriteLine();
            Console.Write(vector.ToString());
            return;
        }
        public void Print()
        {
            Console.WriteLine();
            Console.Write(this.ToString());
            return;
        }
        public override string ToString()
        {
            bool neg = (this.Value.Min() < 0);

            int displace = new int[] { ((int)Max()).ToString().Length, ((int)Min()).ToString().Length}.Max();
            int maxchar = $"{displace:0.00}".Length;

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < this.Value.Length; i++)
            {
                if ((i % this.Columns == 0) && i != 0)
                {
                    stringBuilder.AppendLine();
                }

                string val = $"{this.Value[i]:0.00}";
                int disp = displace - ((int)Math.Floor(MathF.Abs(this.Value[i]))).ToString().Length;

                stringBuilder.AppendFormat($"| {Util.PadBoth(val, maxchar, disp , this.Value[i] < 0f)} |");
            }

            return stringBuilder.AppendLine().ToString();
        }
        public string ToCSV()
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool is1D = !(this.Columns == 1);
            for (int i = 0; i < this.Value.Length; i++)
            {
                if ((i % this.Columns == 0) && is1D && i != 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append($"{this.Value[i].ToString()},");
            }
            return stringBuilder.ToString();
        }




        // PROPERTIES
        public int RowCount()
        {
            if (this.Columns == 1)
            {
                return 1;
            }
            return this.Value.Length / this.Columns;
        }
        public int ColumnCount()
        {
            if (this.Columns == 1)
            {
                return 0;
            }
            return this.Columns;
        }
        public int Length()
        {
            return this.Value.Length;
        }
        public (int,int) Shape()
        {
            return (this.RowCount(), this.ColumnCount());
        }
        public float Max()
        {
            return this.Value.Max();
        }
        public float Min()
        {
            return this.Value.Min();
        }
        public float Mean()
        {
            return this.Value.Average();
        }
        public float Range()
        {
            return this.Value.Max() - this.Value.Min();
        }
        public void Flatten()
        {
            this.Columns = 1;
        }
        public float Sum()
        {
            return this.Value.Sum();
        }



        // CREATION
        #region
        /// <summary>
        /// Creates a UNIFORM Vector where all values are equal to Value
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Size"></param>
        /// <param name="Columns"></param>
        /// <returns></returns>
        public static Vector Fill(GPU gpu, float Value, int Length, int Columns = 1)
        {
            return new Vector(gpu, Enumerable.Repeat(Value, Length).ToArray(), Columns);
        }
        /// <summary>
        /// Sets all values in THIS Vector to value, of a set size and columns
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Size"></param>
        /// <param name="Columns"></param>
        /// <param name="inplace"></param>
        public void _Fill(float Value, int Length, int Columns = 1, bool inplace= true)
        {
            this.Value = Enumerable.Repeat(Value, Length).ToArray();
            this.Columns = Columns;
        }
        public static Vector Zeros(GPU gpu, int Length, int Columns)
        {
            return new Vector(gpu, new float[Length], 1);
        }
        public void _Zeros(int Length, int Columns)
        {
            this.Value = new float[Length];
            this.Columns = Columns;
            return;
        }
        public static Vector Ones(GPU gpu, int Length, int Columns)
        {
            return new Vector(gpu, Enumerable.Repeat(1f, Length).ToArray(), 1);
        }
        public void _Ones(int Length, int Columns)
        {
            this.Value = Enumerable.Repeat(1f, Length).ToArray();
            this.Columns = Columns;
            return;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startval"></param>
        /// <param name="endval"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns=1)
        {
            float interval = MathF.Abs(endval - startval) / (steps - 1);
            if (endval < startval)
            {
                interval = -interval;
            }
            return new Vector(gpu, (from val in Enumerable.Range(0, steps) select startval + (val * interval)).ToArray(), columns);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startval"></param>
        /// <param name="endval"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Vector Arange(GPU gpu, float startval, float endval, float interval, int columns=1)
        {
            int steps = (int)MathF.Abs((endval - startval) / interval);
            if (endval < startval)
            {
                interval = -interval;
            }
            return new Vector(gpu,(from val in Enumerable.Range(0, steps)
                               select startval + (val * interval)).ToArray(), columns);
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
        public static Vector AccessSlice(Vector vector, int row_col_index, char row_col)
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

            Accelerator gpu = vector.gpu.accelerator;
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

            return new Vector(vector.gpu, Output);
        }
        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of this vector
        /// </summary>
        /// <param name="gpu"></param>
        /// <param name="row_col_index"></param>
        /// <param name="row_col"></param>
        /// <returns></returns>
        public Vector _AccessSlice(int row_col_index, char row_col)
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

            //this is bad and I should feel bad
            Accelerator gpu = this.gpu.accelerator;
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

            return new Vector(this.gpu, Output);
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
            return new Vector(vector.gpu, vector.Value[(row * vector.Columns)..((row + 1) * vector.Columns)], 1);
        }
        /// <summary>
        /// Access a specific row of this Vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public Vector _AccessRow(Vector vector, int row)
        {
            return new Vector(vector.gpu, vector.Value[(row * vector.Columns)..((row + 1) * vector.Columns)], 1);
        }

        #endregion


        // MEMORY ALLOCATION
        /// <summary>
        /// Concatinates VectorB onto the end of VectorA.
        /// Preserves the value of Columns of VectorA.
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        public static Vector Concat(Vector vectorA, Vector vectorB)
        {
            return new Vector(vectorA.gpu, vectorA.Value.Concat(vectorB.Value).ToArray(), vectorA.Columns);
        }
        /// <summary>
        /// Concatinates Vector onto the end of this Vector.
        /// Preserves the value of Columns of this Vector.
        /// </summary>
        /// <param name="vector"></param>
        public void _Concat(Vector vector)
        {
            this.Value = this.Value.Concat(vector.Value).ToArray();
            return;
        }

        /// <summary>
        /// Concatinates VectorB onto the end of VectorA removing any duplicates.
        /// Preserves the value of Columns of VectorA.
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        public static Vector Merge(Vector vectorA, Vector vectorB)
        {
            return new Vector(vectorA.gpu, vectorA.Value.Union(vectorB.Value).ToArray(), vectorA.Columns);
        }
        /// <summary>
        /// Concatinates Vector onto the end of this Vector removing any duplicates.
        /// Preserves the value of Columns of this Vector.
        /// </summary>
        /// <param name="vector"></param>
        public void _Merge(Vector vector)
        {
            this.Value = this.Value.Union(vector.Value).ToArray();
            return;
        }

        public static Vector Append(Vector vectorA, Vector vectorB, char axis)
        {
            if (axis == 'r')
            {
                return Vector.Concat(vectorA,vectorB);
            }

            if (vectorA.RowCount() != vectorB.RowCount() || (vectorA.RowCount() != vectorB.Length() && vectorB.Columns == 1) )
            {
                if (vectorB.Columns == 1)
                {
                    throw new Exception($"Vectors CANNOT be appended;" +
                    $" this array has {vectorA.RowCount()} rows, 1D vector being appended has {vectorB.Length()} Length");
                }

                throw new Exception($"Vectors CANNOT be appended;" +
                $" this array has {vectorA.RowCount()} rows, 2D vector being appended has {vectorB.RowCount()}");

            }


            Accelerator gpu = vectorA.gpu.accelerator;
            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);

            var buffer = gpu.Allocate<float>(vectorB.Value.Length + vectorA.Value.Length); // Output
            var buffer2 = gpu.Allocate<float>(vectorA.Value.Length); // Input
            var buffer3 = gpu.Allocate<float>(vectorB.Value.Length); // Input

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);
            buffer3.MemSetToZero(Stream);

            buffer2.CopyFrom(Stream, vectorA.Value, 0, 0, vectorA.Value.Length);
            buffer3.CopyFrom(Stream, vectorB.Value, 0, 0, vectorB.Value.Length);

            kernelWithStream(Stream, vectorA.RowCount(), buffer.View, buffer2.View, buffer3.View, vectorA.Columns, vectorB.Columns);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            Stream.Dispose();

            return new Vector(vectorA.gpu, Output, vectorA.Columns + vectorB.Columns);
        }
        public void _Append(Vector vector, char axis)
        {
            if (axis == 'r')
            {
                this._Concat(vector);
                return;
            }
            if (this.RowCount() != vector.RowCount() || (this.RowCount() != vector.Length() && vector.Columns == 1))
            {
                if (vector.Columns == 1)
                {
                    throw new Exception($"Vectors CANNOT be appended;" +
                    $" this array has {this.RowCount()} rows, 1D vector being appended has {vector.Length()} Length");
                }

                throw new Exception($"Vectors CANNOT be appended;" +
                $" this array has {this.RowCount()} rows, 2D vector being appended has {vector.RowCount()}");

            }

            Accelerator gpu = this.gpu.accelerator;

            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);

            var buffer = gpu.Allocate<float>(vector.Value.Length + this.Value.Length); // Output
            var buffer2 = gpu.Allocate<float>(this.Value.Length); // Input
            var buffer3 = gpu.Allocate<float>(vector.Value.Length); // Input

            buffer.MemSetToZero(Stream);
            buffer2.MemSetToZero(Stream);
            buffer3.MemSetToZero(Stream);

            buffer2.CopyFrom(Stream, this.Value, 0, 0, this.Value.Length);
            buffer3.CopyFrom(Stream, vector.Value, 0, 0, vector.Value.Length);

            kernelWithStream(Stream, this.RowCount(), buffer.View, buffer2.View, buffer3.View, this.Columns, vector.Columns);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            Stream.Dispose();

            this.Columns += vector.Columns;
            this.Value = Output;
            return;
        }
        static void AppendKernel(Index1 index, ArrayView<float> Output, ArrayView<float> vecA, ArrayView<float> vecB, int vecAcol, int vecBcol)
        {
            for (int i = 0; i < vecAcol; i++)
            {
                Output[index * (vecAcol + vecBcol) + i] = vecA[index * vecAcol + i];
            }
            for (int i = 0; i < vecBcol; i++)
            {
                Output[index * (vecAcol + vecBcol) + i + vecAcol] = vecB[index * vecBcol + i];
            }
        }

        public static Vector Prepend(Vector vectorA, Vector vectorB, char axis)
        {
            return Vector.Append(vectorB, vectorA, axis);
        }
        public void _Prepend(Vector vector, char axis)
        {
            Vector vec = Vector.Append(vector, this, axis);
            this.Value = vec.Value;
            this.Columns = vec.Columns;
            return;
        }

        public static Vector Nan_to_num(Vector vector, float num)
        {
            Accelerator gpu = vector.gpu.accelerator;
            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, float>(Nan_to_numKernel);

            var buffer = gpu.Allocate<float>(vector.Value.Length); // IO

            buffer.MemSetToZero(Stream);

            buffer.CopyFrom(Stream, vector.Value, 0, 0, vector.Value.Length);

            kernelWithStream(Stream, vector.Value.Length, buffer.View, num);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            Stream.Dispose();

            return new Vector(vector.gpu, Output, vector.Columns);
        }
        public void _Nan_to_num(float num)
        {
            Accelerator gpu = this.gpu.accelerator;
            AcceleratorStream Stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, float>(Nan_to_numKernel);

            var buffer = gpu.Allocate<float>(this.Value.Length); // IO

            buffer.MemSetToZero(Stream);

            buffer.CopyFrom(Stream, this.Value, 0, 0, this.Value.Length);

            kernelWithStream(Stream, this.Value.Length, buffer.View, num);

            Stream.Synchronize();

            float[] Output = buffer.GetAsArray(Stream);

            buffer.Dispose();
            Stream.Dispose();

            this.Value = Output;
            return;
        }
        static void Nan_to_numKernel(Index1 index, ArrayView<float> IO, float num)
        {
            if (float.IsNaN(IO[index]) || float.IsInfinity(IO[index]))
            {
                IO[index] = num;
            }
            
        }



        // FUNCTIONS
        public static Vector ConsecutiveOP(Vector vectorA, Vector vectorB, string operation = "*")
        {
            if (vectorA.Value.Length != vectorB.Value.Length)
            {
                throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
            }

            if(vectorA.gpu != vectorB.gpu)
            {
                throw new InvalidOperationException("Vector A GPU and Vector B GPU must match");
            }

            Accelerator gpu = vectorA.gpu.accelerator;

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

            return new Vector(vectorA.gpu, Output, vectorA.Columns);
        }
        public void _ConsecutiveOP(Vector vectorB, string operation = "*")
        {
            if (this.Value.Length != vectorB.Value.Length)
            {
                throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
            }

            Accelerator gpu = vectorB.gpu.accelerator;
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

            this.Value = Output;
            return;
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
        public static Vector ConsecutiveOP(Vector vector, float scalar, string operation = "*")
        {
            Accelerator gpu = vector.gpu.accelerator;
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

            return new Vector(vector.gpu, Output, vector.Columns);
        }
        public void _ConsecutiveOP(Accelerator gpu, float scalar, string operation = "*")
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

            this.Value = Output;
            return;
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


        public static Vector Diff(Vector vectorA)
        {
            if (vectorA.Columns > 1)
            {
                throw new Exception("Diff is for use with 1D Vectors ONLY");
            }

            Accelerator gpu = vectorA.gpu.accelerator;
            AcceleratorStream stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>>(DiffKernel);

            MemoryBuffer<float> buffer = gpu.Allocate<float>(vectorA.Value.Length - 1); // Output
            MemoryBuffer<float> buffer2 = gpu.Allocate<float>(vectorA.Value.Length); //  Input

            buffer.MemSetToZero(stream);
            buffer2.MemSetToZero(stream);

            buffer2.CopyFrom(stream, vectorA.Value, 0, 0, vectorA.Value.Length);

            kernelWithStream(stream, buffer.Length, buffer.View, buffer2.View);

            stream.Synchronize();

            float[] Output = buffer.GetAsArray(stream);

            buffer.Dispose();
            buffer2.Dispose();

            stream.Dispose();

            return new Vector(vectorA.gpu, Output);
        }
        public void _Diff(Accelerator gpu)
        {
            if (this.Columns > 1)
            {
                throw new Exception("Diff is for use with 1D Vectors ONLY");
            }

            AcceleratorStream stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>>(DiffKernel);

            MemoryBuffer<float> buffer = gpu.Allocate<float>(this.Value.Length - 1); // Output
            MemoryBuffer<float> buffer2 = gpu.Allocate<float>(this.Value.Length); //  Input

            buffer.MemSetToZero(stream);
            buffer2.MemSetToZero(stream);

            buffer2.CopyFrom(stream, this.Value, 0, 0, this.Value.Length);

            kernelWithStream(stream, buffer.Length, buffer.View, buffer2.View);

            stream.Synchronize();

            float[] Output = buffer.GetAsArray(stream);

            buffer.Dispose();
            buffer2.Dispose();

            stream.Dispose();

            this.Value = Output;
            return;
        }
        static void DiffKernel(Index1 index, ArrayView<float> Output, ArrayView<float> Input)
        {
            Output[index] = Input[index + 1] - Input[index];
        }


        public static float DotProduct(Vector vectorA, Vector vectorB)
        {
            return ConsecutiveOP(vectorA, vectorB, "*").Value.Sum();
        }
        public static float DotProduct(Vector vectorA, float scalar)
        {
            return ConsecutiveOP(vectorA, scalar).Value.Sum();
        }
        public float DotProduct(Vector vectorB)
        {
            return ConsecutiveOP(this, vectorB, "*").Value.Sum();
        }
        public float DotProduct(float scalar)
        {
            return ConsecutiveOP(this, scalar).Value.Sum();
        }


        public static Vector Normalise(Vector vectorA)
        {
            return ConsecutiveOP(vectorA, 1f / vectorA.Value.Sum(), "*");
        }
        public void _Normalise()
        {
            this.Value = ConsecutiveOP(this, 1f / this.Value.Sum(), "*").Value;
        }

        /// <summary>
        /// Takes the absolute value of all values in the Vector.
        /// IMPORTANT : Use this method for Vectors of Length less than 100,000
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector Abs(Vector vector)
        {
            if (vector.Value.Min() > 0f)
            {
                return vector;
            }

            for (int i = 0; i < vector.Value.Length; i++)
            {
                vector.Value[i] = MathF.Abs(vector.Value[i]);
            }
            return vector;
        }
        /// <summary>
        /// Takes the absolute value of all values in this Vector.
        /// IMPORTANT : Use this method for Vectors of Length less than 100,000
        /// </summary>
        public void _Abs()
        {
            if (this.Value.Min() > 0f)
            {
                return;
            }

            for (int i = 0; i < this.Value.Length; i++)
            {
                this.Value[i] = MathF.Abs(this.Value[i]);
            }
            return;
        }
        /// <summary>
        /// Runs on Accelerator. (GPU : Default)
        /// Takes the absolute value of all the values in the Vector.
        /// IMPORTANT : Use this method for Vectors of Length more than 100,000
        /// </summary>
        /// <param name="gpu"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector AbsX(Vector vector)
        {
            Accelerator gpu = vector.gpu.accelerator;
            AcceleratorStream stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>>(AbsKernel);

            MemoryBuffer<float> buffer = gpu.Allocate<float>(vector.Value.Length); // Input/Output

            buffer.MemSetToZero(stream);

            buffer.CopyFrom(stream, vector.Value, 0, 0, vector.Value.Length);

            kernelWithStream(stream, buffer.Length, buffer.View);

            stream.Synchronize();

            float[] Output = buffer.GetAsArray(stream);

            buffer.Dispose();
            stream.Dispose();

            return new Vector(vector.gpu, Output, vector.Columns);
        }
        /// <summary>
        /// Runs on Accelerator. (GPU : Default)
        /// Takes the absolute value of all the values in this Vector.
        /// IMPORTANT : Use this method for Vectors of Length more than 100,000
        /// </summary>
        /// <param name="gpu"></param>
        public void _AbsX(Accelerator gpu)
        {
            AcceleratorStream stream = gpu.CreateStream();

            var kernelWithStream = gpu.LoadAutoGroupedKernel<Index1, ArrayView<float>>(AbsKernel);

            MemoryBuffer<float> buffer = gpu.Allocate<float>(this.Value.Length); // Input/Output

            buffer.MemSetToZero(stream);

            buffer.CopyFrom(stream, this.Value, 0, 0, this.Value.Length);

            kernelWithStream(stream, buffer.Length, buffer.View);

            stream.Synchronize();

            float[] Output = buffer.GetAsArray(stream);

            buffer.Dispose();
            stream.Dispose();

            this.Value = Output;
            return;
        }
        static void AbsKernel(Index1 index, ArrayView<float> IO)
        {
            IO[index] = XMath.Abs(IO[index]);
        }


        /// <summary>
        /// Determines if All the values in the Vector are Non-Zero
        /// </summary>
        /// <returns></returns>
        public static bool All(Vector vector)
        {
            return !vector.Value.Contains(0f);
        }
        /// <summary>
        /// Determines if All the values in this Vector are Non-Zero
        /// </summary>
        /// <returns></returns>
        public bool All()
        {
            return !this.Value.Contains(0f);
        }





    }



}
