using BAVCL.Core;

namespace BAVCL;

public partial class Vector
{
    public static Vector TransferBuffer(Vector Inheritee, Vector Temp, bool IncColumns = false)
    {
        Inheritee.Gpu.FreeBuffer(Inheritee.ID);
        Residence transferred = Temp.Residence;
        Inheritee.ID = Temp.ID;
        Inheritee.Value = Temp.Value;
        Inheritee.Length = Temp.Length;
        if (IncColumns) { Inheritee.Columns = Temp.Columns; }
        Inheritee.SetResidence(transferred);

        Temp.ID = 0;
        Temp.SetResidence(Residence.Cpu);
        return Inheritee;
    }

    public Vector TransferBuffer(Vector Temp, bool IncColumns = false)
    {
        Gpu.FreeBuffer(ID);
        Residence transferred = Temp.Residence;
        ID = Temp.ID;
        Value = Temp.Value;
        Length = Temp.Length;
        if (IncColumns) { Columns = Temp.Columns; }
        SetResidence(transferred);

        Temp.ID = 0;
        Temp.SetResidence(Residence.Cpu);
        return this;
    }
}
