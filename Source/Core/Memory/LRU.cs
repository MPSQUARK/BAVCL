using BAVCL.Core.Interfaces;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace BAVCL.Core;

internal class LRU : IMemoryManager
{
    readonly object _gate = new();
    public ConcurrentDictionary<uint, CacheEntry> Caches = new();

    readonly Queue<uint> _lru = new();
    protected internal long _memoryUsed = 0;
    protected internal int _liveObjectCount = 0;
    protected internal uint _currentVecId = 0;

    public LRU() { }

    public LRU(long maxMemory, float memoryCap)
    {
        if (memoryCap <= 0f || memoryCap >= 1f)
            throw new Exception($"Memory Cap CANNOT be less than 0 or more than 1. Recieved {memoryCap}");
        AvailableMemory = (long)Math.Round(maxMemory * memoryCap);
    }

    #region Debug
    public HashSet<uint> StoredIDs()
    {
        lock (_gate)
            return [.. _lru];
    }

    public bool IsStored(uint id) => Caches.ContainsKey(id);
    #endregion

    public long AvailableMemory { get; set; } = -1;
    public long MemoryUsed => Interlocked.Read(ref _memoryUsed);

    public (uint, MemoryBuffer) AllocateEmpty<T>(ICacheable cacheable, int length, Accelerator accelerator) where T : unmanaged
    {
        uint id = GenerateId();
        MemoryBuffer1D<T, Stride1D.Dense> buffer;
        long memNeeded = (long)Interop.SizeOf<T>() * (long)length;

        lock (_gate)
        {
            GC(memNeeded);
            UpdateMemoryUsage(memNeeded);
            buffer = accelerator.Allocate1D<T>(length);
            RegisterEntry(id, buffer, cacheable);
        }

        return (id, buffer);
    }

    public (uint, MemoryBuffer) Allocate<T>(ICacheable<T> cacheable, Accelerator accelerator) where T : unmanaged
    {
        ReadOnlySpan<T> values = cacheable.RetrieveReadOnlySpan();
        uint id = GenerateId();
        MemoryBuffer1D<T, Stride1D.Dense> buffer;

        lock (_gate)
        {
            GC(cacheable.MemorySize);
            UpdateMemoryUsage(cacheable.MemorySize);
            buffer = accelerator.Allocate1D<T>(values.Length);
            buffer.AsArrayView<T>(0, values.Length).CopyFromCPU(values);
            RegisterEntry(id, buffer, cacheable);
        }

        return (id, buffer);
    }

    public (uint, MemoryBuffer) Allocate<T>(ICacheable cacheable, T[] values, Accelerator accelerator) where T : unmanaged
    {
        uint id = GenerateId();
        MemoryBuffer1D<T, Stride1D.Dense> buffer;

        lock (_gate)
        {
            GC(cacheable.MemorySize);
            UpdateMemoryUsage(cacheable.MemorySize);
            buffer = accelerator.Allocate1D(values);
            RegisterEntry(id, buffer, cacheable);
        }

        return (id, buffer);
    }

    public MemoryBuffer? GetBuffer(uint id)
    {
        if (TryGetCacheEntry(id, out CacheEntry entry))
            return entry.MemoryBuffer;
        return null;
    }

    public void GC(long memRequired)
    {
        if (memRequired > AvailableMemory)
            throw new Exception($"Cannot cache this data onto the GPU, required memory : {memRequired >> 20} MB, max memory available : {AvailableMemory >> 20} MB.\n " +
                                $"Consider spliting/breaking the data into multiple smaller sets OR \n Caching to a GPU with more available memory.");

        while ((memRequired + MemoryUsed) > AvailableMemory)
        {
            if (_liveObjectCount == 0)
                throw new Exception(
                    $"GPU states {_liveObjectCount} Live Tasks Running, while requiring {memRequired >> 20} MB which is more than available {(AvailableMemory - MemoryUsed) >> 20} MB. Potential cause: memory leak");

            lock (_gate)
            {
                if (!_lru.TryDequeue(out uint Id))
                    throw new Exception($"LRU Empty Cannot Continue DeCaching");

                if (TryGetCacheEntry(Id, out CacheEntry entry))
                {
                    if (IsICacheableLive(entry, Id, syncOnEvict: true)) continue;

                    // Id was already dequeued above; DisposeCacheEntry's RemoveFromLRU is a harmless no-op here.
                    DisposeCacheEntry(entry, Id);
                }
            }
        }
    }

    public uint GCItem(uint Id)
    {
        if (!TryGetCacheEntry(Id, out CacheEntry entry)) return 0;

        lock (_gate)
        {
            if (IsICacheableLive(entry, Id, syncOnEvict: true)) return Id;
            DisposeCacheEntry(entry, Id);
        }

        return 0;
    }

    public uint FreeBuffer(uint Id)
    {
        if (!TryGetCacheEntry(Id, out CacheEntry entry)) return 0;

        lock (_gate)
        {
            if (IsICacheableLive(entry, Id, syncOnEvict: false)) return Id;
            DisposeCacheEntry(entry, Id);
        }

        return 0;
    }

    void DisposeCacheEntry(CacheEntry entry, uint Id)
    {
        if (entry.CachedObjRef.TryGetTarget(out ICacheable? cacheable) && cacheable.ID == Id)
            cacheable.ID = 0;

        entry.MemoryBuffer.Dispose();
        UpdateMemoryUsage(-entry.MemoryBuffer.LengthInBytes);
        SubtractLiveTask();
        Caches.TryRemove(Id, out _);
        RemoveFromLRU(Id);
    }

    bool TryGetCacheEntry(uint id, [NotNullWhen(true)] out CacheEntry entry)
    {
        if (Caches.TryGetValue(id, out CacheEntry? found) && found is not null)
        {
            entry = found;
            return true;
        }

        entry = null!;
        return false;
    }

    bool IsICacheableLive(CacheEntry entry, uint Id, bool syncOnEvict)
    {
        if (!entry.CachedObjRef.TryGetTarget(out ICacheable? cacheable))
            return false;

        if (cacheable.LiveCount > 0 || ResidenceHelper.IsActiveGpu(cacheable.Residence))
        {
            _lru.Enqueue(Id);
            return true;
        }

        if (!syncOnEvict)
            return false;

        if (ResidenceHelper.CanFreeWithoutSync(cacheable.Residence))
            return false;

        cacheable.SyncCPU(entry.MemoryBuffer);
        return false;
    }

    /// <summary>Registers a freshly-allocated buffer under <paramref name="id"/>. Caller must hold <see cref="_gate"/>.</summary>
    void RegisterEntry(uint id, MemoryBuffer buffer, ICacheable cacheable)
    {
        Caches.TryAdd(id, new CacheEntry(buffer, new WeakReference<ICacheable>(cacheable)));
        _lru.Enqueue(id);
        AddLiveTask();
    }

    void RemoveFromLRU(uint Id)
    {
        if (_lru.Count == 0 || !LruContains(Id)) return;

        _lru.TryDequeue(out uint DequeuedId);

        if (DequeuedId == Id) return;

        _lru.Enqueue(DequeuedId);

        for (int i = 0; i < _lru.Count; i++)
        {
            _lru.TryDequeue(out DequeuedId);
            if (Id != DequeuedId)
                _lru.Enqueue(DequeuedId);
        }
    }

    bool LruContains(uint id)
    {
        foreach (uint queued in _lru)
        {
            if (queued == id)
                return true;
        }

        return false;
    }

    uint GenerateId() => Interlocked.Increment(ref _currentVecId);
    void UpdateMemoryUsage(long size) => Interlocked.Add(ref _memoryUsed, size);
    public void AddLiveTask() => Interlocked.Increment(ref _liveObjectCount);
    public void SubtractLiveTask() => Interlocked.Decrement(ref _liveObjectCount);

    public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable<T> cacheable, Accelerator accelerator) where T : unmanaged
    {
        uint id = cacheable.ID;
        if (id == 0) return Allocate(cacheable, accelerator);

        ReadOnlySpan<T> values = cacheable.RetrieveReadOnlySpan();

        lock (_gate)
        {
            if (!TryGetCacheEntry(id, out CacheEntry entry))
                return AllocateFromSpanUnderLock(cacheable, values, accelerator);

            MemoryBuffer buffer = entry.MemoryBuffer;
            if (buffer.Length == values.Length)
            {
                buffer.AsArrayView<T>(0, values.Length).CopyFromCPU(values);
                return (id, buffer);
            }

            DisposeCacheEntry(entry, id);
            return AllocateFromSpanUnderLock(cacheable, values, accelerator);
        }
    }

    public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable cacheable, T[] values, Accelerator accelerator) where T : unmanaged
    {
        uint id = cacheable.ID;
        if (id == 0) return Allocate(cacheable, values, accelerator);

        lock (_gate)
        {
            if (!TryGetCacheEntry(id, out CacheEntry entry))
                return AllocateArrayUnderLock(cacheable, values, accelerator);

            MemoryBuffer buffer = entry.MemoryBuffer;
            if (buffer.Length == values.Length)
            {
                buffer.AsArrayView<T>(0, values.Length).CopyFromCPU(values);
                return (id, buffer);
            }

            DisposeCacheEntry(entry, id);
            return AllocateArrayUnderLock(cacheable, values, accelerator);
        }
    }

    (uint, MemoryBuffer) AllocateFromSpanUnderLock<T>(ICacheable<T> cacheable, ReadOnlySpan<T> values, Accelerator accelerator)
        where T : unmanaged
    {
        uint id = GenerateId();
        MemoryBuffer1D<T, Stride1D.Dense> buffer = accelerator.Allocate1D<T>(values.Length);
        buffer.AsArrayView<T>(0, values.Length).CopyFromCPU(values);
        GC(cacheable.MemorySize);
        UpdateMemoryUsage(cacheable.MemorySize);
        RegisterEntry(id, buffer, cacheable);
        return (id, buffer);
    }

    (uint, MemoryBuffer) AllocateArrayUnderLock<T>(ICacheable cacheable, T[] values, Accelerator accelerator)
        where T : unmanaged
    {
        uint id = GenerateId();
        MemoryBuffer1D<T, Stride1D.Dense> buffer = accelerator.Allocate1D(values);
        GC(cacheable.MemorySize);
        UpdateMemoryUsage(cacheable.MemorySize);
        RegisterEntry(id, buffer, cacheable);
        return (id, buffer);
    }
}
