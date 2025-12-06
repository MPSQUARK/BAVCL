using System;
using BAVCL.Core.Enums;
using BAVCL.Geometric.Enums;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public float GetAt(int row, Coord coord)
    {
        if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
        SyncCPU();
        return Value[row + row + row + (int)coord];
    }

    public float GetAt(int row, Coord coord, IndexingMode mode)
    {
        if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
        if (!mode.HasFlag(IndexingMode.NoCPUSync))
            SyncCPU();
        return Value[row + row + row + (int)coord];
    }

}
