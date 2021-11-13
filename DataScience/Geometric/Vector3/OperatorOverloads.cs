using ILGPU.Runtime;
using System;
using System.Collections.Generic;

namespace DataScience.Geometric
{
    public partial class Vector3
    {

        // Vector3 - Vector3 Interaction
        public static Vector3 operator +(Vector3 vectorA, Vector3 vectorB)
        {
            return OP(vectorA, vectorB, Operations.add);
        }
        public static Vector3 operator -(Vector3 vectorA, Vector3 vectorB)
        {
            return OP(vectorA, vectorB, Operations.subtract);
        }
        public static Vector3 operator *(Vector3 vectorA, Vector3 vectorB)
        {
            return OP(vectorA, vectorB, Operations.multiply);
        }
        public static Vector3 operator /(Vector3 vectorA, Vector3 vectorB)
        {
            return OP(vectorA, vectorB, Operations.divide);
        }
        public static Vector3 operator ^(Vector3 vectorA, Vector3 vectorB)
        {
            return OP(vectorA, vectorB, Operations.pow);
        }


        // Vector3 - Vector Interaction

        // Vector - Vector3 Interaction


        // Vector3 - Float Interaction
        public static Vector3 operator +(Vector3 vector, float scalar)
        {
            return OP(vector, scalar, Operations.add);
        }
        public static Vector3 operator -(Vector3 vector, float scalar)
        {
            return OP(vector, scalar, Operations.subtract);
        }
        public static Vector3 operator *(Vector3 vector, float scalar)
        {
            return OP(vector, scalar, Operations.multiply);
        }
        public static Vector3 operator /(Vector3 vector, float scalar)
        {
            return OP(vector, scalar, Operations.divide);
        }
        public static Vector3 operator ^(Vector3 vector, float scalar)
        {
            return OP(vector, scalar, Operations.pow);
        }


        // Float - Vector3 Interaction
        public static Vector3 operator +(float scalar, Vector3 vector)
        {
            return OP(vector, scalar, Operations.add);
        }
        public static Vector3 operator -(float scalar, Vector3 vector)
        {
            return OP(vector, scalar, Operations.flipSubtract);
        }
        public static Vector3 operator *(float scalar, Vector3 vector)
        {
            return OP(vector, scalar, Operations.multiply);
        }
        public static Vector3 operator /(float scalar, Vector3 vector)
        {
            return OP(vector, scalar, Operations.flipDivide);
        }
        public static Vector3 operator ^(float scalar, Vector3 vector)
        {
            return OP(vector, scalar, Operations.flipPow);
        }

        // Vector3 - Double Interaction

        // Double - Vector3 Interaction


    }



}
