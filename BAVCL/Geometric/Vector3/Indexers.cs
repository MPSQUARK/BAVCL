using BAVCL.Geometric.Enums;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public float this[int i, Coord coord]
    {
        get => GetAt(i, coord);
        set => SetAt(i, coord, value);
    }
}
