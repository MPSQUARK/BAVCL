

namespace DataScience
{
    public partial class Vector
    {
        public static float DotProduct(Vector vectorA, Vector vectorB)
        {
            return ConsecutiveOP(vectorA, vectorB, Operations.multiplication).Sum();
        }
        public static float DotProduct(Vector vectorA, float scalar)
        {
            return ConsecutiveOP(vectorA, scalar, Operations.multiplication).Sum();
        }
        public float DotProduct(Vector vectorB)
        {
            return ConsecutiveOP(this, vectorB, Operations.multiplication).Sum();
        }
        public float DotProduct(float scalar)
        {
            return ConsecutiveOP(this, scalar, Operations.multiplication).Sum();
        }


    }



}
