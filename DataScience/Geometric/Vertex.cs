using ILGPU.Algorithms;
using System;
using BAVCL.Utility;

namespace BAVCL.Geometric
{
    public struct Vertex
    {
        /// <summary>
        /// Based on Vector Implementation produced by NullandKale (vec3 struct)
        /// REFERENCE : 
        /// https://github.com/NullandKale/nano-engine/blob/main/Rendering/DataStructures/Vectors.cs
        /// </summary>


        // VARIABLE BLOCK
        public float[] values;

        public float x 
        {
            get { return values[0]; }
            set { values[0] = value; } 
        }
        public float y
        {
            get { return values[1]; }
            set { values[1] = value; }
        }
        public float z
        {
            get { return values[2]; }
            set { values[2] = value; }
        }

        // CONSTRUCTORS
        public Vertex(float x, float y, float z)
        {
            this.values = new float[3] { x, y, z };
        }
        public Vertex(double x, double y, double z)
        {
            this.values = new float[3] { (float)x, (float)y, (float)z };
        }
        public Vertex(float[] array)
        {
            if (array.Length != 3) { throw new Exception($"To Make A Vertex You MUST Provide 3 Values. Recieved : {array.Length}"); }
            this.values = array;
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

        // Check Equivalency
        public bool Equals(Vertex vert)
        {
            return (Util.IsClose(this.x, vert.x)) && (Util.IsClose(this.y, vert.y)) && (Util.IsClose(this.z, vert.z));
        }
        public bool Equals(Vertex vertA, Vertex vertB)
        {
            return (Util.IsClose(vertA.x, vertB.x)) && (Util.IsClose(vertA.y, vertB.y)) && (Util.IsClose(vertA.z, vertB.z));
        }


        // Copy Vertex
        public Vertex Copy()
        {
            return new Vertex(this.values[..]);
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
        public static Vertex operator -(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.x - vertB.x,
                vertA.y - vertB.y,
                vertA.z - vertB.z);
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

        // Conversion Operators
        #region
        public static implicit operator float[](Vertex vert)
        {
            return vert.values;
        }
        public static implicit operator Vertex(float[] values)
        {
            return new Vertex(values);
        }
        public static implicit operator Vertex(Vector3 vector)
        {
            return new Vertex(vector.Value);
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


        // Methods (non-static, with static counterparts)
        public float Distance(Vertex vert)
        {
            System.Numerics.Vector3 vec = new System.Numerics.Vector3(values[0], values[1], values[2]);
            System.Numerics.Vector3 vec2 = new System.Numerics.Vector3(vert.values[0], vert.values[1], vert.values[2]);
            return System.Numerics.Vector3.Distance(vec, vec2);
        }
        public void Fract_IP()
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

        public float Dot(float Scalar)
        {
            return this.x * Scalar + this.y * Scalar + this.z * Scalar;
        }
        public void Cross_IP(Vertex vert)
        {
            this.x =   this.y * vert.z - this.z * vert.y;
            this.y = -(this.x * vert.z - this.z * vert.x);
            this.z =   this.x * vert.y - this.y * vert.x;
            return;
        }
        public void UnitVector_IP()
        {
            float InvMag = 1f/this.Magnitude();
            this.x *= InvMag;
            this.y *= InvMag;
            this.z *= InvMag;
            return;
        }
        public void Aces_approx_IP()
        {
            float a = 2.51f;
            float b = 0.03f;
            float c = 2.43f;
            float d = 0.59f;
            float e = 0.14f;

            this.x = XMath.Clamp(((this.x * 0.6f * (a * this.x * 0.6f + b)) / (this.x * 0.6f * (c * this.x * 0.6f + d) + e)), 0f, 1f);
            this.y = XMath.Clamp(((this.y * 0.6f * (a * this.y * 0.6f + b)) / (this.y * 0.6f * (c * this.y * 0.6f + d) + e)), 0f, 1f);
            this.z = XMath.Clamp(((this.z * 0.6f * (a * this.z * 0.6f + b)) / (this.z * 0.6f * (c * this.z * 0.6f + d) + e)), 0f, 1f);
            return;
        }
        public void Reinhard_IP()
        {
            this.x /= (1f + this.x);
            this.y /= (1f + this.y);
            this.z /= (1f + this.z);
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
        public static float Dot(Vertex vertA, float Scalar) 
        {
            return vertA.x * Scalar + vertA.y * Scalar + vertA.z * Scalar;
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
        public static Vertex Reflect(Vertex normal, Vertex incomming)
        {
            return UnitVector(incomming - normal * 2f * Dot(incomming, normal));
        }
        public static Vertex Refract(Vertex v, Vertex n, float niOverNt)
        {
            v.UnitVector_IP();
            float dt = Dot(v, n);
            float discriminant = 1f - niOverNt * niOverNt * (1f - dt * dt);

            if (discriminant <= 0) { return v; }

            //return niOverNt * (v - (n * dt)) - n * XMath.Sqrt(discriminant);
            return niOverNt * v + n * (-(niOverNt * dt + XMath.Sqrt(discriminant)));
        }
        public static float NormalReflectance(Vertex normal, Vertex incomming, float iorFrom, float iorTo)
        {
            float iorRatio = iorFrom / iorTo;
            float cosThetaI = -Dot(normal, incomming);
            float sinThetaTSquared = iorRatio * iorRatio * (1 - cosThetaI * cosThetaI);
            if (sinThetaTSquared > 1) { return 1f; }

            float cosThetaT = XMath.Sqrt(1 - sinThetaTSquared);
            // rPerpendicular == rParallel ?? VERY STRANGE
            float rPerpendicular = (iorFrom * cosThetaI - iorTo * cosThetaT) / (iorFrom * cosThetaI + iorTo * cosThetaT);
            float rParallel = (iorFrom * cosThetaI - iorTo * cosThetaT) / (iorFrom * cosThetaI + iorTo * cosThetaT);

            return (rPerpendicular * rPerpendicular + rParallel * rParallel) * 0.5f;
        }
        public static Vertex Aces_approx(Vertex vert)
        {
            vert.Aces_approx_IP();
            return vert;
        }
        public static Vertex Reinhard(Vertex vert)
        {
            vert.Reinhard_IP();
            return vert;
        }

        




    }


}
