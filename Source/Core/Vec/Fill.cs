using System.Linq;
using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    /// <summary>
    /// Creates a UNIFORM Vector where all values are equal to Value
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="Length"></param>
    /// <param name="Columns"></param>
    /// <returns></returns>
    public static Vec<T> Fill(GPU gpu, T Value, int Length, uint Columns = 1, bool Cache = true) =>
        new(gpu, Enumerable.Repeat(Value, Length).ToArray(), Columns, Cache);


    /// <summary>
    /// Sets all values in THIS Vector to value, of a set size and columns
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="Length"></param>
    /// <param name="Columns"></param>
    public Vec<T> Fill_IP(T Value, int Length, uint Columns = 1)
    {
        UpdateCache(Enumerable.Repeat(Value, Length).ToArray());
        this.Columns = Columns;
        return this;
    }

}
