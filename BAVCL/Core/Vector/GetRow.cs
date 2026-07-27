namespace BAVCL
{
    public partial class Vector
    {

        public static float[] GetRowAsArray(Vector vector, int row)
        {
            vector.SyncCPU();
            return vector.Value[(row * vector.Columns)..(++row * vector.Columns)];
        }


        public float[] GetRowAsArray(int row)
        {
            SyncCPU();
            return Value[(row * Columns)..(++row * Columns)];
        }

        public float[] GetRowAsArray(int row, bool noSync)
        {
            return Value[(row * Columns)..(++row * Columns)];
        }


        public static Vector GetRowAsVector(Vector vector, int row)
        {
            vector.SyncCPU();
            return new Vector(vector.Gpu, vector.Value[(row * vector.Columns)..(++row * vector.Columns)]);
        }


        public Vector GetRowAsVector(int row)
        {
            SyncCPU();
            return new Vector(Gpu, Value[(row * Columns)..(++row * Columns)]);
        }


    }
}
