using System.Linq;

namespace BAVCL;

public partial class Vector
{
    // TODO: Reduce the number of copies made here due to ToArray()
    public static Vector Append(Vector vectorA, Vector vectorB) =>
        new(vectorA.Gpu, vectorA.ToArray().Concat(vectorB.ToArray()).ToArray(), vectorA.Columns);

    public Vector Append_IP(Vector vector)
    {
        using (CpuScope(syncOnDispose: true))
        {
            Value = ToArray().Concat(vector.ToArray()).ToArray();
            Length = Value.Length;
        }

        return this;
    }

    public static Vector Prepend(Vector vectorA, Vector vectorB) => Append(vectorB, vectorA);
}
