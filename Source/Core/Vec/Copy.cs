using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public Vec<T> Copy(bool Cache = true)
    {
        if (ID == 0)
            return new Vec<T>(Gpu, Values[..], Columns, Cache);

        return new Vec<T>(Gpu, Pull(), Columns, Cache);
    }
}
