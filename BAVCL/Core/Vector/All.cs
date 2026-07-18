using System;

namespace BAVCL;

public partial class Vector
{
    public static bool All(Vector vector)
    {
        vector.SyncCPU();
        return All(vector.GetCpuReadOnlySpan());
    }

    public bool All()
    {
        SyncCPU();
        return All(GetCpuReadOnlySpan());
    }

    static bool All(ReadOnlySpan<float> data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == 0f)
                return false;
        }

        return true;
    }
}
