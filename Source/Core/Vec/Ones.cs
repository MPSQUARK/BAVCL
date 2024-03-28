using System.Linq;
using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public static Vec<T> Ones(GPU gpu, int Length, uint Columns = 1) =>
        new(gpu, Enumerable.Repeat(T.One, Length).ToArray(), Columns);

    public Vec<T> Ones_IP(int Length, uint Columns = 1)
    {
        UpdateCache(Enumerable.Repeat(T.One, Length).ToArray());
        this.Columns = Columns;
        return this;
    }
}
