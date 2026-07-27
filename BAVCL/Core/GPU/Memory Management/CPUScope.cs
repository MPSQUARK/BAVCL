using System;

namespace BAVCL.Core;

public readonly ref struct CPUScope<T> : IDisposable where T : class, ICacheable
{
    private readonly T _cacheableData;

    public CPUScope(T cacheableData)
    {
        _cacheableData = cacheableData;
        _cacheableData.SyncCPU();
    }

    public void Dispose() => _cacheableData.UpdateCache();
}

public readonly ref struct CPUMultiScope<T> where T : class, ICacheable
{
    private readonly ICacheable[] _cacheableData;

    public CPUMultiScope(params ICacheable[] cacheableData)
    {
        _cacheableData = cacheableData;
        for (int i = 0; i < _cacheableData.Length; i++)
            _cacheableData[i].SyncCPU();
    }

    public void Dispose()
    {
        for (int i = 0; i < _cacheableData.Length; i++)
            _cacheableData[i].UpdateCache();
    }

}