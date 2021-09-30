

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Zeros(GPU gpu, int Length, int Columns = 1)
        {
            return new Vector(gpu, new float[Length], Columns);
        }
        public void Zeros_IP(int Length, int Columns = 1)
        {
            this.Value = new float[Length];
            this.Columns = Columns;
            return;
        }
    }
}
