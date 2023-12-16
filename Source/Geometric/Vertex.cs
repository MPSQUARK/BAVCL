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

        public float X 
        {
            get { return values[0]; }
            set { values[0] = value; } 
        }
        public float Y
        {
            get { return values[1]; }
            set { values[1] = value; }
        }
        public float Z
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
            return $"[ x : {X:0.00} || y : {Y:0.00} || z : {Z:0.00} ]";
        }
        public void Print()
        {
            Console.WriteLine(this.ToString());
        }

        // Check Equivalency
        public bool Equals(Vertex vert)
        {
            return (Util.IsClose(this.X, vert.X)) && (Util.IsClose(this.Y, vert.Y)) && (Util.IsClose(this.Z, vert.Z));
        }
        public static bool Equals(Vertex vertA, Vertex vertB)
        {
            return (Util.IsClose(vertA.X, vertB.X)) && (Util.IsClose(vertA.Y, vertB.Y)) && (Util.IsClose(vertA.Z, vertB.Z));
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
            return new Vertex(-vert.X, -vert.Y, -vert.Z);
        }
        public static Vertex operator -(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.X - vertB.X,
                vertA.Y - vertB.Y,
                vertA.Z - vertB.Z);
        }
        public static Vertex operator +(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.X + vertB.X,
                vertA.Y + vertB.Y,
                vertA.Z + vertB.Z);
        }
        public static Vertex operator +(Vertex vertA, float Scalar)
        {
            return new Vertex(
                vertA.X + Scalar,
                vertA.Y + Scalar,
                vertA.Z + Scalar);
        }
        public static Vertex operator +(float Scalar, Vertex vertA)
        {
            return new Vertex(
                vertA.X + Scalar,
                vertA.Y + Scalar,
                vertA.Z + Scalar);
        }
        public static Vertex operator *(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.X * vertB.X,
                vertA.Y * vertB.Y,
                vertA.Z * vertB.Z);
        }
        public static Vertex operator *(Vertex vertA, float Scalar)
        {
            return new Vertex(
                vertA.X * Scalar,
                vertA.Y * Scalar,
                vertA.Z * Scalar);
        }
        public static Vertex operator *(float Scalar, Vertex vertA)
        {
            return new Vertex(
                vertA.X * Scalar,
                vertA.Y * Scalar,
                vertA.Z * Scalar);
        }
        public static Vertex operator /(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                vertA.X / vertB.X,
                vertA.Y / vertB.Y,
                vertA.Z / vertB.Z);
        }
        public static Vertex operator /(float Scalar, Vertex vert)
        {
            return new Vertex(
                Scalar / vert.X,
                Scalar / vert.Y,
                Scalar / vert.Z);
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
            return XMath.Sqrt(X * X + Y * Y + Z * Z);
        }
        public float MagnitudeSquared()
        {
            return (X * X + Y * Y + Z * Z);
        }


        // Methods (non-static, with static counterparts)
        public float Distance(Vertex vert)
        {
            System.Numerics.Vector3 vec = new(values[0], values[1], values[2]);
            System.Numerics.Vector3 vec2 = new(vert.values[0], vert.values[1], vert.values[2]);
            return System.Numerics.Vector3.Distance(vec, vec2);
        }
        public void Fract_IP()
        {
            this.X -= XMath.Floor(X);
            this.Y -= XMath.Floor(Y);
            this.Z -= XMath.Floor(Z);
            return;
        }

        public float Dot(Vertex vertB)
        {
            return this.X * vertB.X + this.Y * vertB.Y + this.Z * vertB.Z;
        }

        public float Dot(float Scalar)
        {
            return this.X * Scalar + this.Y * Scalar + this.Z * Scalar;
        }
        public void Cross_IP(Vertex vert)
        {
            this.X =   this.Y * vert.Z - this.Z * vert.Y;
            this.Y = -(this.X * vert.Z - this.Z * vert.X);
            this.Z =   this.X * vert.Y - this.Y * vert.X;
            return;
        }
        public void UnitVector_IP()
        {
            float InvMag = 1f/this.Magnitude();
            this.X *= InvMag;
            this.Y *= InvMag;
            this.Z *= InvMag;
            return;
        }
        public void Aces_approx_IP()
        {
            float a = 2.51f;
            float b = 0.03f;
            float c = 2.43f;
            float d = 0.59f;
            float e = 0.14f;

            this.X = XMath.Clamp(((this.X * 0.6f * (a * this.X * 0.6f + b)) / (this.X * 0.6f * (c * this.X * 0.6f + d) + e)), 0f, 1f);
            this.Y = XMath.Clamp(((this.Y * 0.6f * (a * this.Y * 0.6f + b)) / (this.Y * 0.6f * (c * this.Y * 0.6f + d) + e)), 0f, 1f);
            this.Z = XMath.Clamp(((this.Z * 0.6f * (a * this.Z * 0.6f + b)) / (this.Z * 0.6f * (c * this.Z * 0.6f + d) + e)), 0f, 1f);
            return;
        }
        public void Reinhard_IP()
        {
            this.X /= (1f + this.X);
            this.Y /= (1f + this.Y);
            this.Z /= (1f + this.Z);
            return;
        }




        // Methods (static)
        public static Vertex SetX(Vertex vert, float x)
        {
            return new Vertex(x, vert.Y, vert.Z);
        }
        public static Vertex SetY(Vertex vert, float y)
        {
            return new Vertex(vert.X, y, vert.Z);
        }
        public static Vertex SetZ(Vertex vert, float z)
        {
            return new Vertex(vert.X, vert.Y, z);
        }
        
        public static float Distance(Vertex vertA, Vertex vertB)
        {
            float dx = vertA.X - vertB.X;
            float dy = vertA.Y - vertB.Y;
            float dz = vertA.Z - vertB.Z;

            return XMath.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        public static Vertex Fract(Vertex vert)
        {
            return new Vertex(
                vert.X - XMath.Floor(vert.X),
                vert.Y - XMath.Floor(vert.Y),
                vert.Z - XMath.Floor(vert.Z));
        }
        public static float Dot(Vertex vertA, Vertex vertB)
        {
            return vertA.X * vertB.X + vertA.Y * vertB.Y + vertA.Z * vertB.Z;
        }
        public static float Dot(Vertex vertA, float Scalar) 
        {
            return vertA.X * Scalar + vertA.Y * Scalar + vertA.Z * Scalar;
        }
        public static Vertex Cross(Vertex vertA, Vertex vertB)
        {
            return new Vertex(
                  vertA.Y * vertB.Z - vertA.Z * vertB.Y,
                -(vertA.X * vertB.Z - vertA.Z * vertB.X),
                  vertA.X * vertB.Y - vertA.Y * vertB.X);
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
