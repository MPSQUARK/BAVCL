using System;
using BAVCL.Core.Enums;
using BAVCL.Geometric.Enums;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public void SetAt(int row, Coord coord, float value)
    {
        if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
        Value[row + row + row + (int)coord] = value;
    }

    public void SetAt(int row, Coord coord, IndexingMode mode, float value)
    {
        if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
        if (mode.HasFlag(IndexingMode.SyncCPU))
            SyncCPU();
        Value[row + row + row + (int)coord] = value;
        if (mode.HasFlag(IndexingMode.SyncGPU))
            UpdateCache();
    }

}