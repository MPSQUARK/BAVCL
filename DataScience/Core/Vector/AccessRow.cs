

namespace DataScience
{
    public partial class Vector
    {

        public static Vector AccessRow(Vector vector, int row)
        {
            return new Vector(vector.gpu, vector.Value[(row * vector.Columns)..((row + 1) * vector.Columns)], 1);
        }

    }
}
