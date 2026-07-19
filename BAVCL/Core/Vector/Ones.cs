using System.Linq;

namespace BAVCL;

public partial class Vector
{
    public static Vector Ones(GPU gpu, int length, int columns = 0) =>
        new(gpu, Enumerable.Repeat(1f, length).ToArray(), columns);

    public Vector Ones_IP(int length, int columns = 0)
    {
        using (this.CpuScopeAndSync())
        {
            Value = Enumerable.Repeat(1f, length).ToArray();
            Length = Value.Length;
            Columns = columns;
        }

        return this;
    }
}
