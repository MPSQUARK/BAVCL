﻿using System.Threading;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public void IncrementLiveCount()
        {
            Interlocked.Increment(ref _livecount);
        }
        public void DecrementLiveCount()
        {
            Interlocked.Decrement(ref _livecount);
        }

    }


}
