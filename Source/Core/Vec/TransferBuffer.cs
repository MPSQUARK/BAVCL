using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public static Vec<T> TransferBuffer(Vec<T> Inheritee, Vec<T> Temp, bool IncColumns = false)
    {
        Inheritee.Gpu.GCItem(Inheritee.ID);
        Inheritee.ID = Temp.ID;
        Inheritee.Values = Temp.Values;
        if (IncColumns) { Inheritee.Columns = Temp.Columns; }

        Temp.ID = 0;
        return Inheritee;
    }

    public Vec<T> TransferBuffer(Vec<T> Temp, bool IncColumns = false)
    {
        Gpu.GCItem(ID);
        ID = Temp.ID;
        Values = Temp.Values;
        if (IncColumns) { Columns = Temp.Columns; }

        Temp.ID = 0;
        return this;
    }
}
