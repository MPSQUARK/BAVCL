using System.Linq;
using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    /// <summary>
    /// Determines if All the values in the Vector are Non-Zero
    /// </summary>
    /// <returns></returns>
    public static bool All(Vec<T> vector)
    {
        if (vector.ID != 0)
            return !vector.Pull().Contains(T.Zero);

        return !vector.Values.Contains(T.Zero);
    }

    /// <summary>
    /// Determines if All the values in this Vector are Non-Zero
    /// </summary>
    /// <returns></returns>
    public bool All()
    {
        if (ID != 0)
            return !Pull().Contains(T.Zero);
        return !Values.Contains(T.Zero);
    }
}
