using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataScience.Utility;

namespace DataScience
{
    /// <summary>
    /// 
    /// </summary>
    public class Vector3 : VectorBase<float>
    {
        // VARIABLE BLOCK
        public override float[] Value { get; set; }
        public override int Columns { get; protected set; }

        // CONSTRUCTOR
        public Vector3(GPU gpu, float[] value, bool cache = false)
        {
            this.gpu = gpu;
            this.Value = value;
            if (value.Length % 3 != 0) { throw new Exception($"Geometric 3D Vectors MUST have 3 columns, and value length MUST be a multiple of 3 instead recieved : {value.Length}."); }
            this.Columns = 3;
            if (cache)
            {
                this.Cache();
            }
        }


        public uint Cache()
        {
            this.Id = this.gpu.Cache(this.Value);
            return this.Id;
        }
        public void Dispose()
        {
            if (this.Id != 0)
            {
                this.gpu.DeCache((uint)this.Id);
            }
            this.Id = 0;
            return;
        }



        // Enum for (x,y,z)
        public enum Coord
        {
            x = 0,
            y = 1,
            z = 2,
        }


        // Create Vector3
        public static Vector3 Fill(GPU gpu, float Value, int Length, int Columns = 1, bool cache = true)
        {
            return new Vector3(gpu, Enumerable.Repeat(Value, Length).ToArray(), cache);
        }
        public static Vector3 Zeros(GPU gpu, int Length, bool cache = true)
        {
            return new Vector3(gpu, new float[Length], cache);
        }




        // DATA Management
        public Vector3 Copy()
        {
            return new Vector3(this.gpu, this.Value[..]);
        }

        public float GetCoOrd(Coord xyz)
        {
            return this.Value[(int)xyz];
        }
        public static Vector3 AccessRow(Vector3 vector, int vert_row)
        {
            return new Vector3(vector.gpu, vector.Value[(vert_row * 3)..((vert_row + 1) * 3)]);
        }
        public void AccessRow(int vert_row)
        {
            this.Value = this.Value[(vert_row * 3)..((vert_row + 1) * 3)];
            return;
        }


        // Appening Verticies
        public static Vector3 Concat(Vector3 vectorA, Vertex vertA)
        {
            Vector3 vector = vectorA.Copy();
            vector.Concat_IP(vertA);
            return vector;
        }
        public static Vector3 Concat(Vector3 vectorA, Vertex[] vertices)
        {
            Vector3 vector = vectorA.Copy();
            vector.Concat_IP(vertices);
            return vector;
        }
        public static Vector3 Concat(Vector3 vectorA, List<Vertex> vertices)
        {
            Vector3 vector = vectorA.Copy();
            vector.Concat_IP(vertices);
            return vector;
        }
        public void Concat_IP(Vertex vertA)
        {
            this.Value = this.Value.Append(vertA.x).Append(vertA.y).Append(vertA.z).ToArray();
            return;
        }
        public void Concat_IP(Vertex[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                this.Value = this.Value.Append(vertices[i].x).Append(vertices[i].y).Append(vertices[i].z).ToArray();
            }
            return;
        }
        public void Concat_IP(List<Vertex> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                this.Value = this.Value.Append(vertices[i].x).Append(vertices[i].y).Append(vertices[i].z).ToArray();
            }
            return;
        }

        // Appening Vector3's 
        public static Vector3 Concat(Vector3 vectorA, Vector3 vectorB)
        {
            Vector3 vector = vectorA.Copy();
            vector.Concat_IP(vectorB);
            return vector;
        }
        public static Vector3 Concat(Vector3 vectorA, Vector3[] vectors)
        {
            Vector3 vector = vectorA.Copy();
            vector.Concat_IP(vectors);
            return vector;
        }
        public static Vector3 Concat(Vector3 vectorA, List<Vector3> vectors)
        {
            Vector3 vector = vectorA.Copy();
            vector.Concat_IP(vectors);
            return vector;
        }
        public void Concat_IP(Vector3 vector)
        {
            this.Value.Concat(vector.Value);
            return;
        }
        public void Concat_IP(Vector3[] vectors)
        {
            for (int i = 0; i < vectors.Length; i++)
            {
                this.Value.Concat(vectors[i].Value);
            }
            return;
        }
        public void Concat_IP(List<Vector3> vectors)
        {
            for (int i = 0; i < vectors.Count; i++)
            {
                this.Value.Concat(vectors[i].Value);
            }
            return;
        }



        // CONVERT TO STRING
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
        // CONVERT TO GENERIC VECTOR
        public Vector ToVector(bool cache = true)
        {
            return new Vector(this.gpu, this.Value, this.Columns, cache);
        }
        public Vector ToVector(int Columns, bool cache = true)
        {
            return new Vector(this.gpu, this.Value, Columns, cache);
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
        public override float Range()
        {
            return (this.Max() - this.Min());
        }
        public override float Sum()
        {
            return this.Value.Sum();
        }


        #endregion

        // OPERATORS
        #region
        public static Vector3 operator +(Vector3 vectorA, Vertex vertA)
        {
            vectorA.Concat_IP(vertA);
            return vectorA;
        }
        public static Vector3 operator +(Vector3 vectorA, Vertex[] verts)
        {
            vectorA.Concat_IP(verts);
            return vectorA;
        }
        public static Vector3 operator +(Vector3 vectorA, List<Vertex> verts)
        {
            vectorA.Concat_IP(verts);
            return vectorA;
        }




        #endregion


        public static Vector3 CrossProduct(Vector3 VectorA, Vector3 VectorB)
        {
            if (VectorA.Length() != VectorB.Length()) { throw new Exception($"Cannot Cross Product two Vector3's together of different lengths. {VectorA.Length()} != {VectorB.Length()}"); }

            if (VectorA.Length() == 3 && VectorB.Length() == 3)
            {
                float x = VectorA.Value[1] * VectorB.Value[2] - VectorA.Value[2] * VectorB.Value[1];
                float y = VectorA.Value[2] * VectorB.Value[0] - VectorA.Value[0] * VectorB.Value[2];
                float z = VectorA.Value[0] * VectorB.Value[1] - VectorA.Value[1] * VectorB.Value[0];
                return new Vector3(VectorA.gpu, new float[3] { x, y, z });
            }

            var buffer = VectorA.gpu.accelerator.Allocate<float>(VectorA.Value.Length); // OutPut
            var buffer2 = VectorA.gpu.accelerator.Allocate<float>(VectorA.Value.Length); // Input
            var buffer3 = VectorA.gpu.accelerator.Allocate<float>(VectorB.Value.Length); // Input

            buffer2.CopyFrom(VectorA.Value, 0, 0, VectorA.Value.Length);
            buffer3.CopyFrom(VectorB.Value, 0, 0, VectorB.Value.Length);

            VectorA.gpu.crossKernel(VectorA.gpu.accelerator.DefaultStream, VectorA.Value.Length / 3, buffer.View, buffer2.View, buffer3.View);

            VectorA.gpu.accelerator.Synchronize();

            float[] Output = buffer.GetAsArray();

            buffer.Dispose();

            return new Vector3(VectorA.gpu, Output);
        }






    }



}
