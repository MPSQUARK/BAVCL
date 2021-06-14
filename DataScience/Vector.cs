using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataScience.Utility;


namespace DataScience
{

    /// <summary>
    /// Class for 1D and 2D Vector support
    /// Float Precision
    /// </summary>
    public class Vector : VectorBase<float>
    {
        // VARIABLE BLOCK
        public override float[] Value { get; set; }
        public override int Columns { get; protected set; }


        // CONSTRUCTOR
        /// <summary>
        /// Constructs a Vector class object.
        /// </summary>
        /// <param name="gpu">The device to use when computing this Vector.</param>
        /// <param name="values">The array of data contained in this Vector.</param>
        /// <param name="columns">The number of Columns IF this is a 2D Vector, for 1D Vectors use the default Columns = 1</param>
        public Vector(GPU gpu, float[] value, int columns = 1)
        {
            this.gpu = gpu;
            this.Value = value;
            this.Columns = columns;
        }


        // FEATURES
        /* LOG :
         *      - Access Slice                          : IMPLEMENTED   : NEEDS TESTING
         *      - Access Value                          : IMPLEMENTED   : NEEDS TESTING
         *      - Consecutive OP                        : IMPLEMENTED   : NEEDS TESTING
         *      - Dot Product                           : IMPLEMENTED   : NEEDS TESTING
         *      - Fill                                  : IMPLEMENTED   : NEEDS TESTING
         *      - Normalise                             : IMPLEMENTED   : NEEDS TESTING
         *      - Linspace                              : IMPLEMENTED   : NEEDS TESTING
         *      - Arange                                : IMPLEMENTED   : NEEDS TESTING
         *      - Diff                                  : IMPLEMENTED   : NEEDS TESTING
         *      - Reciprocal                            : IMPLEMENTED   : NEEDS TESTING
         *      - Absolute                              : IMPLEMENTED   : NEEDS TESTING
         *      - Reverse                               : IMPLEMENTED   : NEEDS TESTING
        */

        // METHODS


        // ToString override
        public override string ToString()
        {
            bool neg = (this.Value.Min() < 0);

            int displace = new int[] { ((int)Max()).ToString().Length, ((int)Min()).ToString().Length }.Max();
            int maxchar = $"{displace:0.00}".Length;

            if (displace > maxchar)
            {
                int temp = displace;
                displace = maxchar;
                maxchar = temp;
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < this.Value.Length; i++)
            {
                if ((i % this.Columns == 0) && i != 0)
                {
                    stringBuilder.AppendLine();
                }

                string val = $"{this.Value[i]:0.00}";
                int disp = displace - ((int)Math.Floor(MathF.Abs(this.Value[i]))).ToString().Length;

                stringBuilder.AppendFormat($"| {Util.PadBoth(val, maxchar, disp, this.Value[i] < 0f)} |");
            }

            return stringBuilder.AppendLine().ToString();
        }

        // MATHEMATICAL PROPERTIES 
        #region
        public override float Max()
        {
            return this.Value.Max();
        }
        public override float Min()
        {
            return this.Value.Min();
        }
        public override float Mean()
        {
            return this.Value.Average();
        }
        public float Std()
        {
            return XMath.Sqrt(this.Var());
        }
        public float Var()
        {
            return Vector.ConsecutiveOP(this, this.Mean(), Operations.squareOfDiffs).Sum() / this.Length();
        }
        public override float Range()
        {
            return this.Value.Max() - this.Value.Min();
        }
        public override float Sum()
        {
            return this.Value.Sum();
        }


        public void Flatten()
        {
            this.Columns = 1;
        }

        public bool IsRectangular()
        {
            return (this.Length() % this.Columns == 0);
        }



        #endregion


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
        public void Fill_IP(float Value, int Length, int Columns = 1)
        {
            this.Value = Enumerable.Repeat(Value, Length).ToArray();
            this.Columns = Columns;
            return;
        }
        public static Vector Zeros(GPU gpu, int Length, int Columns = 1)
        {
            return new Vector(gpu, new float[Length], Columns);
        }
        public void Zeros_IP(int Length, int Columns = 1)
        {
            this.Value = new float[Length];
            this.Columns = Columns;
            return;
        }
        public static Vector Ones(GPU gpu, int Length, int Columns = 1)
        {
            return new Vector(gpu, Enumerable.Repeat(1f, Length).ToArray(), Columns);
        }
        public void Ones_IP(int Length, int Columns = 1)
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
        public static Vector Linspace(GPU gpu, float startval, float endval, int steps, int Columns = 1)
        {
            if (steps <= 1) { throw new Exception("Cannot make linspace with less than 1 steps"); }
            float interval = (endval - startval) / (steps - 1);
            return new Vector(gpu, (from val in Enumerable.Range(0, steps) select startval + (val * interval)).ToArray(), Columns);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startval"></param>
        /// <param name="endval"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Vector Arange(GPU gpu, float startval, float endval, float interval, int Columns = 1)
        {
            int steps = (int)((endval - startval) / interval);
            if (endval < startval && interval > 0) { steps = Math.Abs(steps); interval = -interval; }
            if (endval % interval != 0) { steps++; }

            return new Vector(gpu, (from val in Enumerable.Range(0, steps)
                                    select startval + (val * interval)).ToArray(), Columns);
        }

        public Vector Copy()
        {
            Vector vec = new Vector(this.gpu, this.Value, this.Columns);
            return vec;
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
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
        /// </summary>
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

            if (row_col == 'r')
            {
                return AccessRow(vector, row_col_index);
            }
            if (row_col == 'c')
            {
                return AccessColumn(vector, row_col_index);
            }

            throw new Exception("Invalid slice char selector, choose 'r' for row or 'c' for column");
        }
        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
        /// </summary>
        /// <param name="row_col_index"></param>
        /// <param name="row_col"></param>
        /// <returns></returns>
        public Vector AccessSlice(int row_col_index, char row_col)
        {
            if (this.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            if (row_col == 'r')
            {
                return AccessRow(row_col_index);
            }
            if (row_col == 'c')
            {
                return AccessColumn(row_col_index);
            }

            throw new Exception("Invalid slice char selector, choose 'r' for row or 'c' for column");
        }



        public static Vector AccessRow(Vector vector, int row)
        {
            return new Vector(vector.gpu, vector.Value[(row * vector.Columns)..((row + 1) * vector.Columns)], 1);
        }
        public Vector AccessRow(int row)
        {
            return new Vector(this.gpu, this.Value[(row * this.Columns)..((row + 1) * this.Columns)], 1);
        }



        public static Vector AccessColumn(Vector vector, int column)
        {
            int[] select = new int[2] { column, vector.Columns};

            var buffer = vector.gpu.accelerator.Allocate<float>(vector.RowCount());     // Output
            var buffer2 = vector.gpu.accelerator.Allocate<float>(vector.Value.Length);  // Input
            var buffer3 = vector.gpu.accelerator.Allocate<int>(2);                      // Config

            buffer2.CopyFrom(vector.Value, 0, 0, vector.Value.Length);
            buffer3.CopyFrom(select, 0, 0, select.Length);

            vector.gpu.accessSliceKernel(vector.gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View);

            vector.gpu.accelerator.Synchronize();

            float[] Output = buffer.GetAsArray();

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();
            
            return new Vector(vector.gpu, Output);
        }
        public Vector AccessColumn(int column)
        {
            int[] select = new int[2] { column, this.Columns };

            var buffer = this.gpu.accelerator.Allocate<float>(this.RowCount());     // Output
            var buffer2 = this.gpu.accelerator.Allocate<float>(this.Value.Length);  // Input
            var buffer3 = this.gpu.accelerator.Allocate<int>(2);                    // Config

            buffer2.CopyFrom(this.Value, 0, 0, this.Value.Length);
            buffer3.CopyFrom(select, 0, 0, select.Length);

            this.gpu.accessSliceKernel(this.gpu.accelerator.DefaultStream, this.RowCount(), buffer.View, buffer2.View, buffer3.View);

            this.gpu.accelerator.Synchronize();

            float[] Output = buffer.GetAsArray();

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            return new Vector(this.gpu, Output);
        }



        #endregion


        // MEMORY ALLOCATION
        #region
        /// <summary>
        /// Concatinates VectorB onto the end of VectorA.
        /// Preserves the value of Columns of VectorA.
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        public static Vector Concat(Vector vectorA, Vector vectorB, char axis='r', bool warp = false)
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

            var buffer = gpu.accelerator.Allocate<float>(vector.Value.Length + this.Value.Length); // Output
            var buffer2 = gpu.accelerator.Allocate<float>(this.Value.Length); // Input
            var buffer3 = gpu.accelerator.Allocate<float>(vector.Value.Length); // Input

            buffer2.CopyFrom(this.Value, 0, 0, this.Value.Length);
            buffer3.CopyFrom(vector.Value, 0, 0, vector.Value.Length);

            gpu.appendKernel(gpu.accelerator.DefaultStream, this.RowCount(), buffer.View, buffer2.View, buffer3.View, this.Columns, vector.Columns);

            gpu.accelerator.Synchronize();

            this.Columns += vector.Columns;
            this.Value = buffer.GetAsArray();

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();
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
        public void Merge_IP(Vector vector)
        {
            this.Value = this.Value.Union(vector.Value).ToArray();
            return;
        }


        public static Vector Append(Vector vectorA, Vector vectorB)
        {
            return new Vector(vectorA.gpu, vectorA.Value.Concat(vectorB.Value).ToArray(), vectorA.Columns);
        }
        public void Append_IP(Vector vector)
        {
            this.Value.Concat(vector.Value).ToArray();
            return;
        }


        public static Vector Prepend(Vector vectorA, Vector vectorB)
        {
            return Vector.Append(vectorB, vectorA);
        }
        public void Prepend_IP(Vector vector, char axis)
        {
            Vector vec = Vector.Append(vector, this);
            this.Value = vec.Value;
            this.Columns = vec.Columns;
            return;
        }

        public static Vector Nan_to_num(Vector vector, float num)
        {
            Vector vec = vector.Copy();
            vec.Nan_to_num_IP(num);
            return vec;
        }
        public void Nan_to_num_IP(float num)
        {
            var buffer = gpu.accelerator.Allocate<float>(this.Value.Length); // IO

            buffer.CopyFrom(this.Value, 0, 0, this.Value.Length);

            gpu.nanToNumKernel(gpu.accelerator.DefaultStream, this.Value.Length, buffer.View, num);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();

            return;
        }



        #endregion


        // CONVERSION
        #region
        public Vector3 ToVector3()
        {
            if (this.Length() % 3 != 0) { throw new Exception("Vector length must be a multiple of 3"); }
            return new Vector3(this.gpu, this.Value);
        }


        #endregion


        // OPERATORS
        #region
        public static Vector operator +(Vector vector)
        {
            return Vector.AbsX(vector);
        }



        public static Vector operator +(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.addition);
        }
        public static Vector operator +(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.addition);
        }
        public static Vector operator +(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.addition);
        }



        public static Vector operator -(Vector vector)
        {
            return Vector.ConsecutiveOP(vector, -1, Operations.multiplication);
        }



        public static Vector operator -(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.subtraction);
        }
        public static Vector operator -(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.subtraction);
        }

        public static Vector operator -(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.flipSubtraction);
        }



        public static Vector operator *(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.multiplication);
        }
        public static Vector operator *(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.multiplication);
        }
        public static Vector operator *(float Scalar, Vector Vector)
        {
            return Vector.ConsecutiveOP(Vector, Scalar, Operations.multiplication);
        }



        public static Vector operator /(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.division);
        }
        public static Vector operator /(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.division);
        }
        public static Vector operator /(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.inverseDivision);
        }



        public static Vector operator ^(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.power);
        }
        public static Vector operator ^(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.power);
        }
        public static Vector operator ^(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.powerFlipped);
        }






        #endregion


        // FUNCTIONS
        public static Vector ConsecutiveOP(Vector vectorA, Vector vectorB, Operations operation)
        {
            Vector vector = vectorA.Copy();
            vector.ConsecutiveOP_IP(vectorB, operation);
            return vector;
        }
        public void ConsecutiveOP_IP(Vector vectorB, Operations operation)
        {
            if (this.Value.Length != vectorB.Value.Length)
            {
                throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
            }

            var buffer = gpu.accelerator.Allocate<float>(this.Value.Length); // Input
            var buffer2 = gpu.accelerator.Allocate<float>(this.Value.Length); // Input
            var buffer3 = gpu.accelerator.Allocate<float>(this.Value.Length); // Output

            buffer.CopyFrom(this.Value, 0, 0, this.Value.Length);
            buffer2.CopyFrom(vectorB.Value, 0, 0, vectorB.Value.Length);

            gpu.consecutiveOperationKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            buffer3.CopyFrom(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();

            return;
        }
        public static Vector ConsecutiveOP(Vector vector, float scalar, Operations operation)
        {
            Vector vec = vector.Copy();
            vec.ConsecutiveOP_IP(scalar, operation);
            return vec;
        }
        public void ConsecutiveOP_IP(float scalar, Operations operation)
        {
            var buffer = gpu.accelerator.Allocate<float>(this.Value.Length);
            var buffer2 = gpu.accelerator.Allocate<float>(this.Value.Length);

            buffer2.CopyFrom(this.Value, 0, 0, this.Value.Length);

            gpu.scalarConsecutiveOperationKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();
            buffer2.Dispose();

            return;
        }


        public static Vector Diff(Vector vector)
        {
            Vector vec = vector.Copy();
            vec.Diff_IP();
            return vec;
        }
        public void Diff_IP()
        {
            if (this.Columns > 1)
            {
                throw new Exception("Diff is for use with 1D Vectors ONLY");
            }

            MemoryBuffer<float> buffer = gpu.accelerator.Allocate<float>(this.Value.Length - 1); // Output
            MemoryBuffer<float> buffer2 = gpu.accelerator.Allocate<float>(this.Value.Length); //  Input

            buffer2.CopyFrom(this.Value, 0, 0, this.Value.Length);

            gpu.diffKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();
            buffer2.Dispose();

            return;
        }


        public static float DotProduct(Vector vectorA, Vector vectorB)
        {
            return ConsecutiveOP(vectorA, vectorB, Operations.multiplication).Value.Sum();
        }
        public static float DotProduct(Vector vectorA, float scalar)
        {
            return ConsecutiveOP(vectorA, scalar, Operations.multiplication).Value.Sum();
        }
        public float DotProduct(Vector vectorB)
        {
            return ConsecutiveOP(this, vectorB, Operations.multiplication).Value.Sum();
        }
        public float DotProduct(float scalar)
        {
            return ConsecutiveOP(this, scalar, Operations.multiplication).Value.Sum();
        }


        public static Vector Normalise(Vector vectorA)
        {
            return ConsecutiveOP(vectorA, 1f / vectorA.Value.Sum(), Operations.multiplication);
        }
        public void Normalise_IP()
        {
            this.Value = ConsecutiveOP(this, 1f / this.Value.Sum(), Operations.multiplication).Value;
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
        public void Abs_IP()
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
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector AbsX(Vector vector)
        {
            MemoryBuffer<float> buffer = vector.gpu.accelerator.Allocate<float>(vector.Value.Length); // Input/Output

            buffer.CopyFrom(vector.Value, 0, 0, vector.Value.Length);

            vector.gpu.absKernel(vector.gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            vector.gpu.accelerator.Synchronize();

            float[] Output = buffer.GetAsArray();

            buffer.Dispose();

            return new Vector(vector.gpu, Output, vector.Columns);
        }
        /// <summary>
        /// Runs on Accelerator. (GPU : Default)
        /// Takes the absolute value of all the values in this Vector.
        /// IMPORTANT : Use this method for Vectors of Length more than 100,000
        /// </summary>
        public void AbsX_IP()
        {
            MemoryBuffer<float> buffer = gpu.accelerator.Allocate<float>(this.Value.Length); // Input/Output

            buffer.CopyFrom(this.Value, 0, 0, this.Value.Length);

            gpu.absKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();

            return;
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


        public static Vector Reciprocal(Vector vector)
        {
            MemoryBuffer<float> buffer = vector.gpu.accelerator.Allocate<float>(vector.Value.Length); // IO

            buffer.CopyFrom(vector.Value, 0, 0, vector.Value.Length);

            vector.gpu.reciprocalKernel(vector.gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            vector.gpu.accelerator.Synchronize();

            float[] Output = buffer.GetAsArray();

            buffer.Dispose();

            return new Vector(vector.gpu, vector.Value, vector.Columns);
        }
        public void Reciprocal_IP()
        {
            MemoryBuffer<float> buffer = gpu.accelerator.Allocate<float>(this.Value.Length); // IO

            buffer.CopyFrom(this.Value, 0, 0, this.Value.Length);

            gpu.reciprocalKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();
            return;
        }


        public static Vector Reverse(Vector vector)
        {
            return new Vector(vector.gpu, vector.Value.Reverse().ToArray(), vector.Columns);
        }
        public void Reverse_IP()
        {
            this.Value = this.Value.Reverse().ToArray();
            return;
        }
        public static Vector ReverseX(Vector vector)
        {
            MemoryBuffer<float> buffer = vector.gpu.accelerator.Allocate<float>(vector.Value.Length); // Output
            MemoryBuffer<float> buffer2 = vector.gpu.accelerator.Allocate<float>(vector.Value.Length); // Input

            buffer2.CopyFrom(vector.Value, 0, 0, vector.Value.Length);

            vector.gpu.reverseKernel(vector.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View);

            vector.gpu.accelerator.Synchronize();

            float[] Output = buffer.GetAsArray();

            buffer.Dispose();
            buffer2.Dispose();

            return new Vector(vector.gpu, Output, vector.Columns);
        }
        public void ReverseX_IP()
        {
            MemoryBuffer<float> buffer = gpu.accelerator.Allocate<float>(this.Value.Length); // Output
            MemoryBuffer<float> buffer2 = gpu.accelerator.Allocate<float>(this.Value.Length); // Input

            buffer2.CopyFrom(this.Value, 0, 0, this.Value.Length);

            gpu.reverseKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();
            buffer2.Dispose();

            return;
        }


        public void Transpose_IP()
        {
            if (this.Columns == 1 || this.Columns >= this.Value.Length) { throw new Exception("Cannot transpose 1D Vector"); }

            MemoryBuffer<float> buffer = gpu.accelerator.Allocate<float>(this.Value.Length); // Output
            MemoryBuffer<float> buffer2 = gpu.accelerator.Allocate<float>(this.Value.Length); // Input

            buffer2.CopyFrom(this.Value, 0, 0, this.Value.Length);

            gpu.transposekernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, this.Columns);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            buffer.Dispose();
            buffer2.Dispose();

            this.Columns = this.RowCount();

            return;
        }
        public static Vector Transpose(Vector vector)
        {
            if (vector.Columns == 1 || vector.Columns >= vector.Value.Length) { throw new Exception("Cannot transpose 1D Vector"); }


            MemoryBuffer<float> buffer = vector.gpu.accelerator.Allocate<float>(vector.Value.Length); // Output
            MemoryBuffer<float> buffer2 = vector.gpu.accelerator.Allocate<float>(vector.Value.Length); // Input

            buffer2.CopyFrom(vector.Value, 0, 0, vector.Value.Length);

            vector.gpu.transposekernel(vector.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, vector.Columns);

            vector.gpu.accelerator.Synchronize();

            float[] Output = buffer.GetAsArray();

            buffer.Dispose();
            buffer2.Dispose();

            return new Vector(vector.gpu, Output, vector.RowCount());

        }


    }



}
