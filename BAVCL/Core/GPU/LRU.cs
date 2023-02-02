using BAVCL.Core.Interfaces;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;


namespace BAVCL.Core
{
    internal class LRU : IMemoryManager
    {
        public ConcurrentDictionary<uint, Cache> Caches = new();

        protected internal ConcurrentQueue<uint> _lru = new();
        protected internal long _memoryUsed = 0;
        protected internal int _liveObjectCount = 0;
        protected internal uint _currentVecId = 0;

        #region Constructor
        public LRU(long maxMemory, float memoryCap)
        {
            if (memoryCap <= 0f || memoryCap >= 1f)
                throw new Exception($"Memory Cap CANNOT be less than 0 or more than 1. Recieved {memoryCap}");
            AvailableMemory = (long)Math.Round(maxMemory * memoryCap);
        }
        #endregion

        #region Debug
        public uint[] StoredIDs() 
        {
            uint[] ids = new uint[_lru.Count];
            _lru.CopyTo(ids, 0 );
			return ids;
        }
        #endregion

        #region Properties
        public long AvailableMemory { get; init; }
        /// <summary>
        /// Thread Safe Memory Used Read
        /// </summary>
        public long MemoryUsed => Interlocked.Read(ref _memoryUsed);
        public long MaxMemory { get; init; }
        #endregion

        #region Allocate
        public (uint, MemoryBuffer) AllocateEmpty<T>(ICacheable cacheable, int length, Accelerator accelerator) where T : unmanaged
        {
            uint id = GenerateId();
            MemoryBuffer1D<T, Stride1D.Dense> buffer;

            long memNeeded = (long)Interop.SizeOf<T>() * (long)length;

            lock (this)
            {
                GC(memNeeded);
                UpdateMemoryUsage(memNeeded);

                buffer = accelerator.Allocate1D<T>(length);
                Caches.TryAdd(id, new Cache(buffer, new WeakReference<ICacheable>(cacheable)));
                _lru.Enqueue(id);
            }
            AddLiveTask();
            return (id, buffer);
        }
        public (uint, MemoryBuffer) Allocate<T>(ICacheable<T> Cacheable, Accelerator accelerator) where T : unmanaged
        {
            uint id = GenerateId();
            MemoryBuffer1D<T, Stride1D.Dense> buffer;

            lock (this)
            {
                GC(Cacheable.MemorySize);
                UpdateMemoryUsage(Cacheable.MemorySize);

                buffer = accelerator.Allocate1D(Cacheable.GetValues());
                Caches.TryAdd(id, new Cache(buffer, new WeakReference<ICacheable>(Cacheable)));
                _lru.Enqueue(id);
            }

            AddLiveTask();
            return (id, buffer);
        }
        public (uint, MemoryBuffer) Allocate<T>(ICacheable Cacheable, T[] values, Accelerator accelerator) where T : unmanaged
        {
            uint id = GenerateId();
            MemoryBuffer1D<T, Stride1D.Dense> buffer;

            lock (this)
            {
                GC(Cacheable.MemorySize);
                UpdateMemoryUsage(Cacheable.MemorySize);
                buffer = accelerator.Allocate1D(values);
                Caches.TryAdd(id, new Cache(buffer, new WeakReference<ICacheable>(Cacheable)));
                _lru.Enqueue(id);
            }

            AddLiveTask();
            return (id, buffer);
        }
        #endregion

        #region Get
        public MemoryBuffer GetBuffer(uint id)
        {
            if (Caches.TryGetValue(id, out Cache cache))
                return cache.MemoryBuffer;
            return null;
        }
        #endregion

        #region Garbage Collect
        public void GC(long memRequired)
        {
            // Check if the memory required doesn't exceed the Maximum available
            if (memRequired > AvailableMemory)
                throw new Exception($"Cannot cache this data onto the GPU, required memory : {memRequired >> 20} MB, max memory available : {AvailableMemory >> 20} MB.\n " +
                                    $"Consider spliting/breaking the data into multiple smaller sets OR \n Caching to a GPU with more available memory.");

            // Keep decaching untill enough space is made to accomodate the data
            while ((memRequired + MemoryUsed) > AvailableMemory)
            {

                if (_liveObjectCount == 0)
                    throw new Exception(
                        $"GPU states {_liveObjectCount} Live Tasks Running, while requiring {memRequired >> 20} MB which is more than available {(AvailableMemory - MemoryUsed) >> 20} MB. Potential cause: memory leak");

                lock (this)
                {
                    // Get the ID of the last item
                    if (!_lru.TryDequeue(out uint Id)) throw new Exception($"LRU Empty Cannot Continue DeCaching");

                    // Try Get Reference to and the object of ICacheable
                    if (Caches.TryGetValue(Id, out Cache cache))
                    {
                        if (IsICacheableLive(cache, Id)) continue;    

                        cache.MemoryBuffer.Dispose();
                        UpdateMemoryUsage(-cache.MemoryBuffer.LengthInBytes);
                        SubtractLiveTask();
                        Caches.TryRemove(Id, out _);
                    }
                }
            }
        }

        public uint GCItem(uint Id)
        {
            if (!Caches.TryGetValue(Id, out Cache cache)) return 0;

            lock (this)
            {
                if (IsICacheableLive(cache, Id)) return Id;

                cache.MemoryBuffer.Dispose();
                UpdateMemoryUsage(-cache.MemoryBuffer.LengthInBytes);
                SubtractLiveTask();
                Caches.TryRemove(Id, out _);
                RemoveFromLRU(Id);
            }

            return 0;
        }

        /// <summary>
        /// If ICachable is live, it will re-add it back to the LRU
        /// If not live, will sync the data back to the cpu
        /// If not present, will say data can be decached
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        private bool IsICacheableLive(Cache cache, uint Id)
        {
            if (!cache.CachedObjRef.TryGetTarget(out ICacheable cacheable))
                return false;
            
            if (cacheable.LiveCount == 0)
            {
                cacheable.SyncCPU(cache.MemoryBuffer);
                return false;
            }

            _lru.Enqueue(Id);
            return true;
        }

        private void RemoveFromLRU(uint Id)
        {
            if (_lru.IsEmpty || !_lru.Contains(Id)) return;

            _lru.TryDequeue(out uint DequeuedId);

            if (DequeuedId == Id) return;

            // Put back the De-queued Id since it didn't match the Disposed Id
            _lru.Enqueue(DequeuedId);

            // shuffle through LRU to remove the disposed Id
            for (int i = 0; i < _lru.Count; i++)
            {
                _lru.TryDequeue(out DequeuedId);

                // Id matching the one disposed will not be re-enqueued 
                // Order will be preserved
                if (Id != DequeuedId)
                {
                    _lru.Enqueue(DequeuedId);
                }
            }
            return;
        }

        #endregion

        #region Helpers
        private uint GenerateId() => Interlocked.Increment(ref _currentVecId);
        private void UpdateMemoryUsage(long size) => Interlocked.Add(ref _memoryUsed, size);
        public void AddLiveTask() => Interlocked.Increment(ref _liveObjectCount);
        public void SubtractLiveTask() => Interlocked.Decrement(ref _liveObjectCount);
        #endregion

        #region Update
        public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable<T> cacheable, Accelerator accelerator) where T : unmanaged
        {
            // If not Cached
            uint id = cacheable.ID;
            if (id == 0) return Allocate(cacheable, accelerator);

            // If not found in cache
            if (!Caches.TryGetValue(id, out Cache cache)) return Allocate(cacheable, accelerator);
            MemoryBuffer buffer = cache.MemoryBuffer;

            if (buffer != null) return Allocate(cacheable, accelerator);

            // If lengths match
            T[] values = cacheable.GetValues();
            if (buffer.Length == values.Length)
            {
                buffer.AsArrayView<T>(0, values.Length).CopyFromCPU(values);
                cache.MemoryBuffer = buffer;
                Caches.AddOrUpdate(id, cache, (id, oldcache) => cache);
                return (id, buffer);
            }

            // If lengths don't match
            GCItem(id);
            return Allocate(cacheable,accelerator);
        }
        public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable cacheable, T[] values, Accelerator accelerator) where T : unmanaged
        {
            // If not cached
            uint id = cacheable.ID;
            if (id == 0) return Allocate(cacheable, values, accelerator);

            // If not found in cache
            if (!Caches.TryGetValue(id, out Cache cache)) return Allocate(cacheable, values, accelerator);
            MemoryBuffer buffer = cache.MemoryBuffer;

            if (buffer == null) return Allocate(cacheable, values, accelerator);

            // If lengths match
            if (buffer.Length == values.Length)
            {
                buffer.AsArrayView<T>(0, values.Length).CopyFromCPU(values);
                cache.MemoryBuffer = buffer;
                    Caches.AddOrUpdate(id, cache, (id, oldcache) => cache);
                return (id, buffer);
            }

            // If lengths don't match
            GCItem(id);
            return Allocate(cacheable, values, accelerator);

        }
        #endregion
    }
}
