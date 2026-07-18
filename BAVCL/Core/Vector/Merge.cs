using System;
using System.Linq;
using BAVCL.Core;

namespace BAVCL;

public partial class Vector
{
    public static Vector Merge(Vector vectorA, Vector vectorB) =>
        new(vectorA.Gpu, vectorA.RetrieveReadOnlySpan().ToArray().Union(vectorB.RetrieveReadOnlySpan().ToArray()).ToArray(), vectorA.Columns);

    public Vector Merge_IP(Vector vector)
    {
        using (CpuScope(syncOnDispose: true))
        {
            // TODO: Performance can still be improved
            ReadOnlySpan<float> left = GetCpuReadOnlySpan();
            ReadOnlySpan<float> right = vector.RetrieveReadOnlySpan();
            Value = left.ToArray().Union(right.ToArray()).ToArray();
            Length = Value.Length;
        }

        return this;
    }
}
