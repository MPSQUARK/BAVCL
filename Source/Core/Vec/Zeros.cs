using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public static Vec<T> Zeros(GPU gpu, int Length, int Columns = 1) => new(gpu, Length, Columns);

    public Vec<T> Zeros_IP(int Length, int Columns = 1)
    {
        UpdateCache(new T[Length]);
        this.Columns = Columns;
        return this;
    }
}
