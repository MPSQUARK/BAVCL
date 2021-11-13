using System;

namespace DataScience.Geometric
{

    public partial class Vector3
    {
        public float GetValue(int i)
        {
            if (i < 0 || i > _length) { throw new IndexOutOfRangeException(); }
            SyncCPU();
            return this.Value[i];
        }
        public float GetValue(int row, Coord coord)
        {
            if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
            SyncCPU();
            return this.Value[row + row + row + (int)coord];
        }
        public float GetValue(int i, IndexingMode mode)
        {
            switch (mode)
            {
                case IndexingMode.Normal:
                    return GetValue(i);
                case IndexingMode.NoCPUSync:
                    if (i < 0 || i > _length) { throw new IndexOutOfRangeException(); }
                    return this.Value[i];
                case IndexingMode.NoGPUSync:
                    throw new Exception("Get values does not utilize GPUSync. Method call ambigious. Please use another mode.");
                case IndexingMode.NoSync:
                    if (i < 0 || i > _length) { throw new IndexOutOfRangeException(); }
                    return this.Value[i];
                default:
                    return GetValue(i);
            }
        }
        public float GetValue(int row, Coord coord, IndexingMode mode)
        {
            switch (mode)
            {
                case IndexingMode.Normal:
                    return GetValue(row, coord);
                case IndexingMode.NoCPUSync:
                    if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
                    return this.Value[row + row + row + (int)coord];
                case IndexingMode.NoGPUSync:
                    throw new Exception("Get values does not utilize GPUSync. Method call ambigious. Please use another mode.");
                case IndexingMode.NoSync:
                    if (row < 0 || row > RowCount()) { throw new IndexOutOfRangeException(); }
                    return this.Value[row + row + row + (int)coord];
                default:
                    return GetValue(row, coord);
            }
        }





    }


}
