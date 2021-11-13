using System;

namespace BAVCL.Geometric
{

    public partial class Vector3
    {

        public void SetValue(int i, float value)
        {
            if (i < 0 || i > _length) { throw new IndexOutOfRangeException(); }
            SyncCPU();
            this.Value[i] = value;
            UpdateCache();
        }
        public void SetValue(int row, Coord coord, float value)
        {
            if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
            SyncCPU();
            this.Value[row+row+row + (int)coord] = value;
            UpdateCache();
        }
        public void SetValue(int i, IndexingMode mode, float value)
        {
            switch (mode)
            {
                case IndexingMode.Normal:
                    SetValue(i, value);
                    break;
                case IndexingMode.NoCPUSync:
                    if (i < 0 || i > _length) { throw new IndexOutOfRangeException(); }
                    this.Value[i] = value;
                    UpdateCache();
                    break;
                case IndexingMode.NoGPUSync:
                    if (i < 0 || i > _length) { throw new IndexOutOfRangeException(); }
                    SyncCPU();
                    this.Value[i] = value;
                    break;
                case IndexingMode.NoSync:
                    if (i < 0 || i > _length) { throw new IndexOutOfRangeException(); }
                    this.Value[i] = value;
                    break;
                default:
                    SetValue(i, value);
                    break;
            }
        }
        public void SetValue(int row, Coord coord, IndexingMode mode, float value)
        {
            switch (mode)
            {
                case IndexingMode.Normal:
                    SetValue(row, coord, value);
                    break;
                case IndexingMode.NoCPUSync:
                    if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
                    this.Value[row + row + row + (int)coord] = value;
                    UpdateCache();
                    break;
                case IndexingMode.NoGPUSync:
                    if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
                    SyncCPU();
                    this.Value[row + row + row + (int)coord] = value;
                    break;
                case IndexingMode.NoSync:
                    if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
                    this.Value[row + row + row + (int)coord] = value;
                    break;
                default:
                    SetValue(row, coord, value);
                    break;
            }
        }



    }


}