using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public void DeCache()
    {
        // If the vector is not cached - it's rechnically already decached
        if (ID == 0) return;

        // If the vector is live - Fail
        if (LiveCount != 0) return;

        // Else Decache
        Values = Pull();
        ID = Gpu.GCItem(ID);
    }
}
