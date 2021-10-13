

namespace DataScience
{
    public partial class Vector
    {

        public static Vector AccessRow(Vector vector, int row)
        {
            if (vector._id != 0)
            {
                return new Vector(vector.gpu, vector.Pull()[(row * vector.Columns)..(++row * vector.Columns)], 1);
            }
            return new Vector(vector.gpu, vector.Value[(row * vector.Columns)..(++row * vector.Columns)], 1);
        }


    }
}
