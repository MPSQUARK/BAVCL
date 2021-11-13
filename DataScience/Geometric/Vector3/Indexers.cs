using System;

namespace BAVCL.Geometric
{

    public partial class Vector3
    {
        // Enum for (x,y,z)
        public enum Coord
        {
            x = 1,
            y = 2,
            z = 3,
        }

        public enum IndexingMode
        {
            Normal = 1,
            NoCPUSync = 2,
            NoGPUSync = 3,
            NoSync = 4,
        }

        public float this[int i]
        {
            get => GetValue(i);
            set => SetValue(i, value);
        } 

        public float this[int i, Coord coord]
        {
            get => GetValue(i, coord);
            set => SetValue(i, coord, value);
        }

        public float this[int i, IndexingMode mode]
        {
            get => GetValue(i, mode);
            set => SetValue(i, mode, value);
        }
        public float this[int i, Coord coord, IndexingMode mode]
        {
            get => GetValue(i, coord, mode);
            set => SetValue(i, coord, mode, value);
        }



    }


}
