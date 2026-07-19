using System;
using System.Collections.Generic;
using System.Linq;
using BAVCL.Core;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public static Vector3 Concat(Vector3 vectorA, Vertex vertA) =>
        vectorA.Copy().Concat_IP(vertA);

    public static Vector3 Concat(Vector3 vectorA, Vertex[] vertices) =>
        vectorA.Copy().Concat_IP(vertices);

    public static Vector3 Concat(Vector3 vectorA, List<Vertex> vertices) =>
        vectorA.Copy().Concat_IP(vertices);

    public Vector3 Concat_IP(Vertex vertA)
    {
        using (this.CpuScopeAndSync())
        {
            // TODO: Can be optimised.
            ReadOnlySpan<float> left = GetCpuReadOnlySpan();
            Value = left.ToArray().Append(vertA.X).Append(vertA.Y).Append(vertA.Z).ToArray();
            Length = Value.Length;
        }

        return this;
    }

    public Vector3 Concat_IP(Vertex[] vertices)
    {
        using (this.CpuScopeAndSync())
        {
            // TODO: can be optimised
            ReadOnlySpan<float> left = GetCpuReadOnlySpan();
            Value = vertices.Aggregate(left.ToArray(), (current, vert) =>
                current.Append(vert.X).Append(vert.Y).Append(vert.Z).ToArray());
            Length = Value.Length;
        }

        return this;
    }

    public Vector3 Concat_IP(List<Vertex> vertices)
    {
        using (this.CpuScopeAndSync())
        {
            ReadOnlySpan<float> left = GetCpuReadOnlySpan();
            Value = vertices.Aggregate(left.ToArray(), (current, vert) =>
                current.Append(vert.X).Append(vert.Y).Append(vert.Z).ToArray());
            Length = Value.Length;
        }

        return this;
    }

    public static Vector3 Concat(Vector3 vectorA, Vector3 vectorB) =>
        vectorA.Copy().Concat_IP(vectorB);

    public static Vector3 Concat(Vector3 vectorA, Vector3[] vectors) =>
        vectorA.Copy().Concat_IP(vectors);

    public static Vector3 Concat(Vector3 vectorA, List<Vector3> vectors) =>
        vectorA.Copy().Concat_IP(vectors);

    public Vector3 Concat_IP(Vector3 vector)
    {
        using (this.CpuScopeAndSync())
        {
            ReadOnlySpan<float> left = GetCpuReadOnlySpan();
            ReadOnlySpan<float> right = vector.RetrieveReadOnlySpan();
            Value = left.ToArray().Concat(right.ToArray()).ToArray();
            Length = Value.Length;
        }

        return this;
    }

    public Vector3 Concat_IP(Vector3[] vectors)
    {
        using (this.CpuScopeAndSync())
        {
            float[] merged = GetCpuReadOnlySpan().ToArray();
            for (int i = 0; i < vectors.Length; i++)
                merged = merged.Concat(vectors[i].RetrieveReadOnlySpan().ToArray()).ToArray();

            Value = merged;
            Length = Value.Length;
        }

        return this;
    }

    public Vector3 Concat_IP(List<Vector3> vectors)
    {
        using (this.CpuScopeAndSync())
        {
            float[] merged = GetCpuReadOnlySpan().ToArray();
            for (int i = 0; i < vectors.Count; i++)
                merged = merged.Concat(vectors[i].RetrieveReadOnlySpan().ToArray()).ToArray();

            Value = merged;
            Length = Value.Length;
        }

        return this;
    }
}
