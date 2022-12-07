using System.Linq;

namespace BAVCL
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
            SyncCPU();
            vector.SyncCPU();
            this.Value = this.Value.Concat(vector.Value).ToArray();
            UpdateCache();
            return this;
        }


        public static Vector Prepend(Vector vectorA, Vector vectorB) => Append(vectorB, vectorA);


    }
}
