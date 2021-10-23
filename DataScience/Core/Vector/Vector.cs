using ILGPU.Runtime;
using ILGPU.Algorithms;
using System;
using DataScience.Core;

namespace DataScience
{

    /// <summary>
    /// Class for 1D and 2D Vector support
    /// Float Precision
    /// </summary>
    public partial class Vector : VectorBase<float>
    {

        // CONSTRUCTOR
        /// <summary>
        /// Constructs a Vector class object.
        /// </summary>
        /// <param name="gpu">The device to use when computing this Vector.</param>
        /// <param name="values">The array of data contained in this Vector.</param>
        /// <param name="columns">The number of Columns IF this is a 2D Vector, for 1D Vectors use the default Columns = 1</param>
        public Vector(GPU gpu, float[] values, int columns = 1, bool cache = true) : base(gpu, values, columns, cache)
        {
        }

        // METHODS
        public bool Equals(Vector vector)
        {
            SyncCPU();
            vector.SyncCPU();

            if (this._length != vector._length) { return false; }

            for (int i = 0; i < vector._length; i++)
            {
                if (this.Value[i] != vector.Value[i]) { return false; }
            }

            return true;
        }
        public Vector Copy(bool Cache = true)
        {
            if (this._id == 0)
            {
                return new Vector(this.gpu, this.Value[..], this.Columns, Cache);
            }

            return new Vector(this.gpu, this.Pull(), this.Columns, Cache);
        }


        #region "MATHEMATICAL PROPERTIES "
        public override float Mean()
        {
            return Sum() / _length;
        }
        public float Std()
        {
            return XMath.Sqrt(Var());
        }
        public float Var()
        {
            SyncCPU();

            if (this._length < 1e4f)
            {
                int vectorSize = System.Numerics.Vector<float>.Count;
                int i = 0;

                float[] array = this.Value;

                float mean = Mean();

                System.Numerics.Vector<float> meanvec = new System.Numerics.Vector<float>(mean);

                System.Numerics.Vector<float> sumVector = System.Numerics.Vector<float>.Zero;

                for (; i <= array.Length - vectorSize; i += vectorSize)
                {
                    System.Numerics.Vector<float> input = new System.Numerics.Vector<float>(array, i);

                    System.Numerics.Vector<float> difference = input - meanvec;

                    sumVector += (difference * difference);

                }

                float sum = 0;

                for (int j = 0; j < vectorSize; j++)
                {
                    sum += sumVector[j];
                }

                for (; i < array.Length; i++)
                {
                    sum += XMath.Pow((array[i] - mean), 2f);
                }

                return sum / this.Length;
            }

            //Vector diff = Vector.AbsX(this - this.Mean());

            //return (diff * diff).Sum() / this.Length();
            return Vector.OP(this, this.Mean(), Operations.DOTS).Sum() / this._length;
        }
        public override float Range()
        {
            return Max() - Min();
        }
        
        public void Flatten()
        {
            this.Columns = 1;
        }

        #endregion


        #region "CONVERSION"

        public Geometric.Vector3 ToVector3()
        {
            if (this.Length % 3 != 0) { throw new Exception("Vector length must be a multiple of 3"); }
            if (this._id != 0)
            {
                return new Geometric.Vector3(this.gpu, this.Pull());
            }
            return new Geometric.Vector3(this.gpu, this.Value);
        }

        #endregion


        #region "OPERATORS"
        public static Vector operator +(Vector vector)
        {
            return Vector.AbsX(vector);
        }



        public static Vector operator +(Vector vectorA, Vector vectorB)
        {
            return Vector.OP(vectorA, vectorB, Operations.add);
        }
        public static Vector operator +(Vector vector, float Scalar)
        {
            return Vector.OP(vector, Scalar, Operations.add);
        }
        public static Vector operator +(float Scalar, Vector vector)
        {
            return Vector.OP(vector, Scalar, Operations.add);
        }



        public static Vector operator -(Vector vector)
        {
            return Vector.OP(vector, -1, Operations.multiply);
        }



        public static Vector operator -(Vector vectorA, Vector vectorB)
        {
            return Vector.OP(vectorA, vectorB, Operations.subtract);
        }
        public static Vector operator -(Vector vector, float Scalar)
        {
            return Vector.OP(vector, Scalar, Operations.subtract);
        }

        public static Vector operator -(float Scalar, Vector vector)
        {
            return Vector.OP(vector, Scalar, Operations.flipSubtract);
        }



        public static Vector operator *(Vector vectorA, Vector vectorB)
        {
            return Vector.OP(vectorA, vectorB, Operations.multiply);
        }
        public static Vector operator *(Vector vector, float Scalar)
        {
            return Vector.OP(vector, Scalar, Operations.multiply);
        }
        public static Vector operator *(float Scalar, Vector Vector)
        {
            return Vector.OP(Vector, Scalar, Operations.multiply);
        }



        public static Vector operator /(Vector vectorA, Vector vectorB)
        {
            return Vector.OP(vectorA, vectorB, Operations.divide);
        }
        public static Vector operator /(Vector vector, float Scalar)
        {
            return Vector.OP(vector, Scalar, Operations.divide);
        }
        public static Vector operator /(float Scalar, Vector vector)
        {
            return Vector.OP(vector, Scalar, Operations.invDivide);
        }



        public static Vector operator ^(Vector vectorA, Vector vectorB)
        {
            return Vector.OP(vectorA, vectorB, Operations.pow);
        }
        public static Vector operator ^(Vector vector, float Scalar)
        {
            return Vector.OP(vector, Scalar, Operations.pow);
        }
        public static Vector operator ^(float Scalar, Vector vector)
        {
            return Vector.OP(vector, Scalar, Operations.flipPow);
        }





        #endregion



        // FUNCTIONS
        public static Vector OP(Vector vectorA, Vector vectorB, Operations operation, bool Warp = false)
        {
            // Check function conditions
            if (vectorA._length == vectorB._length)
            {
                return _VectorVectorOP(vectorA, vectorB, operation);
            }

            bool ThisLonger = vectorA._length > vectorB._length;


            // If one input is a Vector and other is Matrix
            if ((vectorA.Columns == 1 && vectorB.Columns > 1) || (vectorA.Columns > 1 && vectorB.Columns == 1))
            {
                if (ThisLonger) { return _VectorMatrixOP(vectorB, vectorA, operation); }
                return _VectorMatrixOP(vectorA, vectorB, operation);
            }


            throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
        }

        public Vector OP_IP(Vector vectorB, Operations operation)
        {
            // If the lengths are the same and both 1D vectors
            if (this._length == vectorB._length && vectorB.Columns == 1 && this.Columns == 1)
            {
                this._VectorVectorOP_IP(vectorB, operation);
                return this;
            }

            bool ThisLonger = this.Value.Length > vectorB.Value.Length;

            // If one input is a Vector and other is Matrix
            if ((this.Columns == 1 && vectorB.Columns > 1) || (this.Columns > 1 && vectorB.Columns == 1))
            {
                
                
            }

            throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
        }

        public static Vector OP(Vector vector, float scalar, Operations operation)
        {
            GPU gpu = vector.gpu;

            vector.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new Vector(gpu, new float[vector._length], vector.Columns);

            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> 
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vector.GetBuffer();       // Input

            gpu.scalarConsecOpKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            vector.DecrementLiveCount();
            Output.DecrementLiveCount();

            return Output;
        }

        public Vector OP_IP(float scalar, Operations operation)
        {
            IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = GetBuffer(); // IO

            gpu.scalarConsecOpKernelIP(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, scalar, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            DecrementLiveCount();

            return this;
        }


        internal static Vector _VectorVectorOP(Vector vectorA, Vector vectorB, Operations operation)
        {
            GPU gpu = vectorA.gpu;

            vectorA.IncrementLiveCount();
            vectorB.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new Vector(gpu, new float[vectorA._length], vectorA.Columns);
            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> 
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vectorA.GetBuffer(),      // Input
                buffer3 = vectorB.GetBuffer();      // Input

            // Run the kernel
            gpu.consecOpKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            gpu.accelerator.Synchronize();

            vectorA.DecrementLiveCount();
            vectorB.DecrementLiveCount();
            Output.DecrementLiveCount();

            // Return the result
            return Output;
        }

        internal Vector _VectorVectorOP_IP(Vector vectorB, Operations operation)
        {
            vectorB.IncrementLiveCount();
            IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> 
                buffer = GetBuffer(),          // IO
                buffer2 = vectorB.GetBuffer();      // Input

            // Run the kernel
            gpu.consecOpKernelIP(gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            gpu.accelerator.Synchronize();

            vectorB.DecrementLiveCount();
            DecrementLiveCount();

            return this;
        }

        internal static Vector _VectorMatrixOP(Vector vector, Vector matrix, Operations operation)
        {
            GPU gpu = vector.gpu;

            vector.IncrementLiveCount();
            matrix.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new Vector(gpu, new float[vector._length], vector.Columns);

            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> 
                buffer = Output.GetBuffer(),        // Output
                buffer2 = vector.GetBuffer(),       // Input
                buffer3 = matrix.GetBuffer();       // Input

            // Run the kernel
            gpu.vectormatrixOpKernel(gpu.accelerator.DefaultStream, matrix.RowCount(), buffer.View, buffer2.View, buffer3.View, matrix.Columns, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            gpu.accelerator.Synchronize();

            vector.DecrementLiveCount();
            matrix.DecrementLiveCount();
            Output.DecrementLiveCount();

            // Return the result
            return Output;
        }

        internal Vector _VectorMatrixOP_IP(Vector matrix, Operations operation)
        {

            IncrementLiveCount();
            matrix.IncrementLiveCount();

            // Make the Output Vector
            Vector Output = new Vector(gpu, new float[_length], Columns);

            Output.IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> 
                buffer = Output.GetBuffer(),        // Output
                buffer2 = GetBuffer(),              // Input
                buffer3 = matrix.GetBuffer();       // Input

            // Run the kernel
            gpu.vectormatrixOpKernel(gpu.accelerator.DefaultStream, matrix.RowCount(), buffer.View, buffer2.View, buffer3.View, matrix.Columns, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            gpu.accelerator.Synchronize();

            DecrementLiveCount();
            matrix.DecrementLiveCount();
            Output.DecrementLiveCount();

            return this;
        }


       

    }



}
