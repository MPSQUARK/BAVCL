using System.Linq;
using BAVCL.Core;

namespace BAVCL.Geometric
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

        public static Vector3 AccessRow(Vector3 vector, int vert_row)
        {
            vector.SyncCPU();
            return new Vector3(vector.gpu, vector.Value[(vert_row * 3)..((vert_row + 1) * 3)]);
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




    }



}
