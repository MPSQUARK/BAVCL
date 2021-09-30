using System.Linq;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Append(Vector vectorA, Vector vectorB)
        {
            return new Vector(vectorA.gpu, vectorA.Value.Concat(vectorB.Value).ToArray(), vectorA.Columns);
        }
        public void Append_IP(Vector vector)
        {
            this.Value.Concat(vector.Value).ToArray();
            return;
        }


        public static Vector Prepend(Vector vectorA, Vector vectorB)
        {
            return Vector.Append(vectorB, vectorA);
        }
        public void Prepend_IP(Vector vector, char axis)
        {
            Vector vec = Vector.Append(vector, this);
            this.Value = vec.Value;
            this.Columns = vec.Columns;
            return;
        }


    }
}
