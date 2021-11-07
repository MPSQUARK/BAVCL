using System.Linq;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Append(Vector vectorA, Vector vectorB)
        {
            vectorA.SyncCPU();
            vectorB.SyncCPU();
            return new Vector(vectorA.gpu, vectorA.Value.Concat(vectorB.Value).ToArray(), vectorA.Columns);
        }
        public Vector Append_IP(Vector vector)
        {
            this.SyncCPU();
            vector.SyncCPU();
            this.Value = this.Value.Concat(vector.Value).ToArray();
            this.UpdateCache();
            return this;
        }


        public static Vector Prepend(Vector vectorA, Vector vectorB)
        {
            return Vector.Append(vectorB, vectorA);
        }



    }
}
