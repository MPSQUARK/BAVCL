

namespace DataScience
{
    public partial class Vector
    {
        public static float Dot(Vector vectorA, Vector vectorB)
        {
            return OP(vectorA, vectorB, Operations.multiply).Sum();
        }
        public static float Dot(Vector vectorA, float scalar)
        {
            return OP(vectorA, scalar, Operations.multiply).Sum();
        }
        public float Dot(Vector vectorB)
        {
            return OP(this, vectorB, Operations.multiply).Sum();
        }
        public float Dot(float scalar)
        {
            return OP(this, scalar, Operations.multiply).Sum();
        }


    }



}
