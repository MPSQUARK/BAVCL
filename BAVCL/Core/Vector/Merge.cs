using System.Linq;

namespace BAVCL;

public partial class Vector
{
    public static Vector Merge(Vector vectorA, Vector vectorB) =>
        new(vectorA.Gpu, vectorA.ToArray().Union(vectorB.ToArray()).ToArray(), vectorA.Columns);

    // TODO: Reduce the number of copies made here due to ToArray()
    public Vector Merge_IP(Vector vector)
    {
        using (CpuScope(syncOnDispose: true))
        {
            Value = ToArray().Union(vector.ToArray()).ToArray();
            Length = Value.Length;
        }

        return this;
    }
}
