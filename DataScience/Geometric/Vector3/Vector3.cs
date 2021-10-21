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
            return ToString(2);
        }
        public string ToString(byte decimalplaces = 2)
        {
            // FORMAT : "|__-DIGITS.DECIMALPLACES__|"

            int high = ((int)Max()).ToString().Length;
            int low = (int)Min();
            bool hasnegative = low < 0f;
            bool hasinfinity = Value.Contains(float.PositiveInfinity) || Value.Contains(float.NegativeInfinity) || Value.Contains(float.NaN);
            low = hasnegative ? low.ToString().Length - 1 : low.ToString().Length;
            int digits = high > low ? high : low;

            string format = $"F{decimalplaces}";
            char neg = ' ';
            string val;
            string spaces;

            StringBuilder stringBuilder = new StringBuilder();



            stringBuilder.AppendLine();

            if (hasinfinity)
            {

                if (float.IsNegativeInfinity(Value[0]) || float.IsPositiveInfinity(Value[0]))
                {
                    val = "INF   ";
                }
                else if (float.IsNaN(Value[0]))
                {
                    val = "NaN  ";
                }
                else
                {
                    val = Math.Abs(Value[0]).ToString(format);
                }
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                if (Value[0] < 0f) { neg = '-'; } else { neg = ' '; }
                stringBuilder.Append($"|  {neg}{spaces}{val}  |");

                for (int i = 1; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    if (float.IsNegativeInfinity(Value[i]) || float.IsPositiveInfinity(Value[i]))
                    {
                        val = "INF   ";
                    }
                    else if (float.IsNaN(Value[i]))
                    {
                        val = "NaN   ";
                    }
                    else
                    {
                        val = Math.Abs(Value[i]).ToString(format);
                    }
                    spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                    if (Value[i] < 0f) { neg = '-'; } else { neg = ' '; }
                    stringBuilder.Append($"|  {neg}{spaces}{val}  |");
                }

                return stringBuilder.AppendLine().ToString();
            }

            if (hasnegative)
            {
                val = Math.Abs(Value[0]).ToString(format);
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                if (Value[0] < 0f) { neg = '-'; } else { neg = ' '; }
                stringBuilder.Append($"|  {neg}{spaces}{val}  |");

                for (int i = 1; i < _length; i++)
                {
                    if (i % Columns == 0) { stringBuilder.AppendLine(); }

                    val = Math.Abs(Value[i]).ToString(format);
                    spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                    if (Value[i] < 0f) { neg = '-'; } else { neg = ' '; }
                    stringBuilder.Append($"|  {neg}{spaces}{val}  |");
                }

                return stringBuilder.AppendLine().ToString();
            }

            val = Value[0].ToString(format);
            spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
            stringBuilder.Append($"|  {neg}{spaces}{val}  |");

            for (int i = 1; i < _length; i++)
            {
                if (i % Columns == 0) { stringBuilder.AppendLine(); }

                val = Value[i].ToString(format);
                spaces = new string(' ', digits - (val.Length - decimalplaces - 1));
                stringBuilder.Append($"|   {spaces}{val}  |");
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
