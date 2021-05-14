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
        public Vector3(GPU gpu, float[] value)
        {
            this.gpu = gpu;
            this.Value = value;
            if (value.Length % 3 != 0) { throw new Exception($"Geometric 3D Vectors MUST have 3 columns, and value length MUST be a multiple of 3 instead recieved : {value.Length}."); }
            this.Columns = 3;
        }

        // Enum for (x,y,z)
        public enum Coord
        {
            x,
            y,
            z,
        }

        // ACCESS DATA
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
        public static Vector3 AppendVert(Vector3 vectorA, Vertex vertA)
        {
            float[] values = vectorA.Value.Append(vertA.x).Append(vertA.y).Append(vertA.z).ToArray();
            return new Vector3(vectorA.gpu, values);
        }
        public static Vector3 AppendVert(Vector3 vectorA, Vertex[] vertices)
        {
            float[] values = vectorA.Value;


            for (int i = 0; i < vertices.Length; i++)
            {
                values = values.Append(vertices[i].x).Append(vertices[i].y).Append(vertices[i].z).ToArray();
            }

            return new Vector3(vectorA.gpu, values);
        }
        public static Vector3 AppendVert(Vector3 vectorA, List<Vertex> vertices)
        {
            float[] values = vectorA.Value;
            for (int i = 0; i < vertices.Count; i++)
            {
                values = values.Append(vertices[i].x).Append(vertices[i].y).Append(vertices[i].z).ToArray();
            }

            return new Vector3(vectorA.gpu, values);
        }
        public void _AppendVert(Vertex vertA)
        {
            this.Value = this.Value.Append(vertA.x).Append(vertA.y).Append(vertA.z).ToArray();
            return;
        }
        public void _AppendVert(Vertex[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                this.Value = this.Value.Append(vertices[i].x).Append(vertices[i].y).Append(vertices[i].z).ToArray();
            }
            return;
        }
        public void _AppendVert(List<Vertex> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                this.Value = this.Value.Append(vertices[i].x).Append(vertices[i].y).Append(vertices[i].z).ToArray();
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
        public Vector ToVector()
        {
            return new Vector(this.gpu, this.Value, this.Columns);
        }
        public Vector ToVector(int Columns)
        {
            return new Vector(this.gpu, this.Value, Columns);
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
            vectorA.Value = vectorA.Value.Append(vertA.x).Append(vertA.y).Append(vertA.z).ToArray();
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
