using ILGPU.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public struct Vertex
    {
        /// <summary>
        /// Based on Vector Implementation produced by NullandKale (vec3 struct)
        /// REFERENCE : 
        /// https://github.com/NullandKale/nano-engine/blob/main/Rendering/DataStructures/Vectors.cs
        /// </summary>


        // VARIABLE BLOCK
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        // CONSTRUCTORS
        public Vertex(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vertex(double x, double y, double z)
        {
            this.x = (float)x;
            this.y = (float)y;
            this.z = (float)z;
        }

        // Print + ToString
        public override string ToString()
        {
            return $"[ x : {x:0.00} || y : {y:0.00} || z : {z:0.00} ]";
        }
        public void Print()
        {
            Console.WriteLine(this.ToString());
        }


        // PREMADE VERTICES
        #region
        public static Vertex UP()
        {
            return new Vertex(0f, 1f, 0f);
        }
        public static Vertex DOWN()
        {
            return new Vertex(0f, -1f, 0f);
        }
        public static Vertex FORWARD()
        {
            return new Vertex(1f, 0f, 0f);
        }
        public static Vertex BACKWARD()
        {
            return new Vertex(-1f, 0f, 0f);
        }
        public static Vertex LEFT()
        {
            return new Vertex(0f, 0f, -1f);
        }
        public static Vertex RIGHT()
        {
            return new Vertex(0f, 0f, 1f);
        }
        
        #endregion

        // Operators
        #region
        public static Vertex operator -(Vertex vert)
        {
            return new Vertex(-vert.x, -vert.y, -vert.z);
        }
        public static Vertex operator +(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.x + vertB.x,
                vertA.y + vertB.y,
                vertA.z + vertB.z);
        }
        public static Vertex operator +(Vertex vertA, float Scalar)
        {
            return new Vertex(
                vertA.x + Scalar,
                vertA.y + Scalar,
                vertA.z + Scalar);
        }
        public static Vertex operator +(float Scalar, Vertex vertA)
        {
            return new Vertex(
                vertA.x + Scalar,
                vertA.y + Scalar,
                vertA.z + Scalar);
        }
        public static Vertex operator *(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.x * vertB.x,
                vertA.y * vertB.y,
                vertA.z * vertB.z);
        }
        public static Vertex operator *(Vertex vertA, float Scalar)
        {
            return new Vertex(
                vertA.x * Scalar,
                vertA.y * Scalar,
                vertA.z * Scalar);
        }
        public static Vertex operator *(float Scalar, Vertex vertA)
        {
            return new Vertex(
                vertA.x * Scalar,
                vertA.y * Scalar,
                vertA.z * Scalar);
        }
        public static Vertex operator /(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.x / vertB.x,
                vertA.y / vertB.y,
                vertA.z / vertB.z);
        }
        public static Vertex operator /(float Scalar, Vertex vert)
        {
            return new Vertex(
                Scalar / vert.x,
                Scalar / vert.y,
                Scalar / vert.z);
        }
        public static Vertex operator /(Vertex vertA, float Scalar)
        {
            return vertA * (1f / Scalar);
        }
        
        #endregion



        // Methods (non-static)
        public float Magnitude()
        {
            return XMath.Sqrt(x * x + y * y + z * z);
        }
        public float MagnitudeSquared()
        {
            return (x * x + y * y + z * z);
        }


        // Methods (non-static, static counterparts)
        public void SetX(float x)
        {
            this.x = x;
        }
        public void SetY(float y)
        {
            this.y = y;
        }
        public void SetZ(float z)
        {
            this.z = z;
        }
        public float Distance(Vertex vert)
        {
            float dx = this.x - vert.x;
            float dy = this.y - vert.y;
            float dz = this.z - vert.z;

            return XMath.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        public void _Fract()
        {
            this.x -= XMath.Floor(x);
            this.y -= XMath.Floor(y);
            this.z -= XMath.Floor(z);
            return;
        }
        public float Dot(Vertex vertB)
        {
            return this.x * vertB.x + this.y * vertB.y + this.z * vertB.z;
        }
        public void _Cross(Vertex vert)
        {
            this.x =   this.y * vert.z - this.z * vert.y;
            this.y = -(this.x * vert.z - this.z * vert.x);
            this.z =   this.x * vert.y - this.y * vert.x;
            return;
        }
        public void _UnitVector()
        {
            float InvMag = 1f/this.Magnitude();
            this.x *= InvMag;
            this.y *= InvMag;
            this.z *= InvMag;
            return;
        }



        // Methods (static)
        public static Vertex SetX(Vertex vert, float x)
        {
            return new Vertex(x, vert.y, vert.z);
        }
        public static Vertex SetY(Vertex vert, float y)
        {
            return new Vertex(vert.x, y, vert.z);
        }
        public static Vertex SetZ(Vertex vert, float z)
        {
            return new Vertex(vert.x, vert.y, z);
        }
        
        public static float Distance(Vertex vertA, Vertex vertB)
        {
            float dx = vertA.x - vertB.x;
            float dy = vertA.y - vertB.y;
            float dz = vertA.z - vertB.z;

            return XMath.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        public static Vertex Fract(Vertex vert)
        {
            return new Vertex(
                vert.x - XMath.Floor(vert.x),
                vert.y - XMath.Floor(vert.y),
                vert.z - XMath.Floor(vert.z));
        }
        public static float Dot(Vertex vertA, Vertex vertB)
        {
            return vertA.x * vertB.x + vertA.y * vertB.y + vertA.z * vertB.z;
        }
        public static Vertex Cross(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                  vertA.y * vertB.z - vertA.z * vertB.y,
                -(vertA.x * vertB.z - vertA.z * vertB.x),
                  vertA.x * vertB.y - vertA.y * vertB.x);
        }
        public static Vertex UnitVector(Vertex vertex)
        {
            return vertex / vertex.Magnitude();
        }







    }


}
