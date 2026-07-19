using System.Linq;

namespace BAVCL;

public partial class Vector
{
    public static Vector Fill(GPU gpu, float value, int length, int columns = 0, bool cache = true) =>
        new(gpu, Enumerable.Repeat(value, length).ToArray(), columns, cache);

    public Vector Fill_IP(float value, int length, int columns = 0)
    {
        using (this.CpuScopeAndSync())
        {
            Value = Enumerable.Repeat(value, length).ToArray();
            Length = Value.Length;
            Columns = columns;
        }

        return this;
    }
}
