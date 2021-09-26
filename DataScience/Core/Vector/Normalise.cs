

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Normalise(Vector vectorA)
        {
            return ConsecutiveOP(vectorA, 1f / vectorA.Sum(), Operations.multiplication);
        }
        public void Normalise_IP()
        {
            ConsecutiveOP_IP(1f / this.Sum(), Operations.multiplication);
        }


    }
}
