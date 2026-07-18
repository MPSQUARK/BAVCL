using System;

namespace BAVCL;

public partial class Vector
{
    public static bool All(Vector vector) => All(vector.RetrieveReadOnlySpan());

    public bool All() => All(RetrieveReadOnlySpan());

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
