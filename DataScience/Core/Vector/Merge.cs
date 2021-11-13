using System.Linq;


namespace BAVCL
{
    public partial class Vector
    {
        /// <summary>
        /// Concatinates VectorB onto the end of VectorA removing any duplicates.
        /// Preserves the value of Columns of VectorA.
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        public static Vector Merge(Vector vectorA, Vector vectorB)
        {
            vectorA.SyncCPU();
            vectorB.SyncCPU();
            return new Vector(vectorA.gpu, vectorA.Value.Union(vectorB.Value).ToArray(), vectorA.Columns);
        }
        /// <summary>
        /// Concatinates Vector onto the end of this Vector removing any duplicates.
        /// Preserves the value of Columns of this Vector.
        /// </summary>
        /// <param name="vector"></param>
        public Vector Merge_IP(Vector vector)
        {
            SyncCPU();
            vector.SyncCPU();
            UpdateCache(this.Value.Union(vector.Value).ToArray());
            return this;
        }


    }
}
