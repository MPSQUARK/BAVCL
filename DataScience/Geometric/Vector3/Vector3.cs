using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataScience.Utility;

namespace DataScience.Geometric
{

    public partial class Vector3 : VectorBase<float>
    {
        #region "Variables"
        public override int Columns { get { return _columns; } set { _columns = 3; } }
        #endregion

        // CONSTRUCTOR
        public Vector3(GPU gpu, float[] value, bool cache = true) : base(gpu, value, 3, true)
        {
        }


        // Enum for (x,y,z)
        public enum Coord
        {
            x = 1,
            y = 2,
            z = 4,
        }


        // Create Vector3
        public static Vector3 Fill(GPU gpu, float Value, int Length)
        {
            return new Vector3(gpu, Enumerable.Repeat(Value, Length).ToArray());
        }
        public static Vector3 Zeros(GPU gpu, int Length)
        {
            return new Vector3(gpu, new float[Length]);
        }


        // DATA Management
        public Vector3 Copy()
        {
            if (_id != 0)
            {
                return new Vector3(gpu, Pull());
            }
            return new Vector3(gpu, Value[..]);
        }

        public float GetCoOrd(Coord xyz)
        {
            SyncCPU();
            return this.Value[(int)xyz];
        }
        public static Vector3 AccessRow(Vector3 vector, int vert_row)
        {
            vector.SyncCPU();
            return new Vector3(vector.gpu, vector.Value[(vert_row * 3)..((vert_row + 1) * 3)]);
        }



        // CONVERT TO STRING
        public override string ToString()
        {
            bool neg = (Min() < 0);

            int displace = new int[] { ((int)Max()).ToString().Length, ((int)Min()).ToString().Length }.Max();
            int maxchar = $"{displace:0.00}".Length;

            if (displace > maxchar)
            {
                int temp = displace;
                displace = maxchar;
                maxchar = temp;
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < this._length; i++)
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
            if (_id != 0)
            {
                return new Vector(this.gpu, Pull(), this.Columns, cache);
            }
            return new Vector(this.gpu, this.Value, this.Columns, cache);
        }
        public Vector ToVector(int Columns, bool cache = true)
        {
            if (_id != 0)
            {
                return new Vector(this.gpu, Pull(), this.Columns, cache);
            }
            return new Vector(this.gpu, this.Value, Columns, cache);
        }


        // MATHEMATICAL PROPERTIES 
        #region
        public override float Mean()
        {
            SyncCPU();
            return this.Value.Average();
        }
        public override float Range()
        {
            return Max() - Min();
        }
        public override float Sum()
        {
            SyncCPU();
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










    }



}
