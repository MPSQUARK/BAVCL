using System;
using System.Linq;
using BAVCL.Core;

namespace BAVCL;

public partial class Vector
{
    public static Vector Append(Vector vectorA, Vector vectorB) =>
        new(vectorA.Gpu, vectorA.RetrieveReadOnlySpan().ToArray().Concat(vectorB.RetrieveReadOnlySpan().ToArray()).ToArray(), vectorA.Columns);

    public Vector Append_IP(Vector vector)
    {
        using (CpuScope(syncOnDispose: true))
        {
            // TODO: Performance can still be improved
            ReadOnlySpan<float> left = GetCpuReadOnlySpan();
            ReadOnlySpan<float> right = vector.RetrieveReadOnlySpan();
            Value = left.ToArray().Concat(right.ToArray()).ToArray();
            Length = Value.Length;
        }

        return this;
    }

    public static Vector Prepend(Vector vectorA, Vector vectorB) => Append(vectorB, vectorA);
}
