using BAVCL.Core.Enums;
using BAVCL.Geometric.Enums;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public float this[int i, Coord coord]
    {
        get => GetAt(i, coord);
        set => SetAt(i, coord, value);
    }
    public float this[int i, Coord coord, IndexingMode mode]
    {
        get => GetAt(i, coord, mode);
        set => SetAt(i, coord, mode, value);
    }
}