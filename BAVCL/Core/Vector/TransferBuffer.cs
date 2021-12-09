namespace BAVCL
{
    public partial class Vector
    {
        public static Vector TransferBuffer(Vector Inheritee, Vector Temp, bool IncColumns = false)
        {
            Inheritee.gpu.DeCache(Inheritee._id);
            Inheritee._id = Temp._id;
            Inheritee._length = Temp._length;
            if (IncColumns) { Inheritee._columns = Temp._columns; }

            Temp._id = 0;
            return Inheritee;
        }

        public Vector TransferBuffer(Vector Temp, bool IncColumns=false)
        {
            gpu.DeCache(_id);
            _id = Temp._id;
            _length = Temp._length;
            if (IncColumns) { _columns = Temp._columns; }

            Temp._id = 0;
            return this;
        }

    }
}
