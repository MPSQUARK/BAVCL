using BAVCL.Geometric.Enums;
using System;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public float GetAt(int row, Coord coord)
    {
        if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
        return GetAt(row, (int)coord);
    }
}
