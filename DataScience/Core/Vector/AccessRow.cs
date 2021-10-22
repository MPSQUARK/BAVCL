namespace DataScience
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

        public static Vector GetRowAsVector(Vector vector, int row)
        {
            vector.SyncCPU();
            return new Vector(vector.gpu, vector.Value[(row * vector.Columns)..(++row * vector.Columns)]);
        }

        public Vector GetRowAsVector(int row)
        {
            SyncCPU();
            return new Vector(gpu, Value[(row * Columns)..(++row * Columns)]);
        }

    }
}
