using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

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



        public static Vector3 CrossProduct(Vector3 vectorA, Vector3 VectorB)
        {
            if (vectorA.Length() != 3 && VectorB.Length() != 3) { throw new Exception("Method currently only supports 1x3 Vector3 CROSS 1x3 Vector3"); }
            float x = vectorA.Value[1] * VectorB.Value[2] - vectorA.Value[2] * VectorB.Value[1];
            float y = vectorA.Value[2] * VectorB.Value[0] - vectorA.Value[0] * VectorB.Value[2];
            float z = vectorA.Value[0] * VectorB.Value[1] - vectorA.Value[1] * VectorB.Value[0];
            return new Vector3(vectorA.gpu, new float[3] { x, y, z });
        }


    }



}
