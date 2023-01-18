namespace BAVCL
{
    public partial class Vector
    {
        public static Vector TransferBuffer(Vector Inheritee, Vector Temp, bool IncColumns = false)
        {
            Inheritee.gpu.DeCache(Inheritee.ID);
            Inheritee.ID = Temp.ID;
            Inheritee.Value = Temp.Value;
            if (IncColumns) { Inheritee.Columns = Temp.Columns; }

            Temp.ID = 0;
            return Inheritee;
        }

        public Vector TransferBuffer(Vector Temp, bool IncColumns=false)
        {
            gpu.DeCache(ID);
            ID = Temp.ID;
            Value = Temp.Value;
            if (IncColumns) { Columns = Temp.Columns; }

            Temp.ID = 0;
            return this;
        }

    }
}
