namespace BAVCL.Geometric;

using System;
using BAVCL.Core;

public sealed partial class Vector3 : VectorBase<float>
{
    public static Vector3 AccessRow(Vector3 vector, int vertRow)
    {
        ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
        float[] row = data.Slice(vertRow + vertRow + vertRow, 3).ToArray();
        return new Vector3(vector.Gpu, row);
    }
}
