namespace BAVCL;

public partial class Vector
{
    public static Vector Zeros(GPU gpu, int length, int columns = 0) =>
        new(gpu, new float[length], columns);

    public Vector Zeros_IP(int length, int columns = 0)
    {
        using (this.CpuScopeAndSync())
        {
            Value = new float[length];
            Length = Value.Length;
            Columns = columns;
        }

        return this;
    }
}
