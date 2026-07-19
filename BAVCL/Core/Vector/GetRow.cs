using System;

namespace BAVCL;

public partial class Vector
{
    public static float[] GetRowAsArray(Vector vector, int row)
    {
        ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
        return data.Slice(row * vector.Columns, vector.Columns).ToArray();
    }

    public float[] GetRowAsArray(int row)
    {
        ReadOnlySpan<float> data = RetrieveReadOnlySpan();
        return data.Slice(row * Columns, Columns).ToArray();
    }

    public float[] GetRowAsArray(int row, bool noSync)
    {
        ReadOnlySpan<float> data = GetCpuReadOnlySpan();
        return data.Slice(row * Columns, Columns).ToArray();
    }

    public static Vector GetRowAsVector(Vector vector, int row)
    {
        float[] rowData = GetRowAsArray(vector, row);
        return new Vector(vector.Gpu, rowData, 0);
    }

    public Vector GetRowAsVector(int row) =>
        new(Gpu, GetRowAsArray(row), 0);
}
