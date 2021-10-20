

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Normalise(Vector vectorA)
        {
            return OP(vectorA, 1f / vectorA.Sum(), Operations.multiply);
        }
        public Vector Normalise_IP()
        {
            return OP_IP(1f / Sum(), Operations.multiply);
        }


    }
}
