using BAVCL.Core;
using BAVCL.Geometric.Enums;
using System;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public void SetAt(int row, Coord coord, float value)
    {
        if (row < 0 || row > RowCount())
            throw new IndexOutOfRangeException();

        SetAt(row, (int)coord, value);
    }
}
