using System;
using BAVCL.Core.Enums;

namespace BAVCL.Core;

public abstract partial class VectorBase<T> : ICacheable<T>, IIO where T : unmanaged
{
    private int GetIndexFromCoordinates(int row, int col)
    {
        var index = row * Columns + col;
        ValidateIndex(index);
        return index;
    }
    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= Length)
            throw new IndexOutOfRangeException($"Index {index} is out of range for vector of length {Length}.");
    }

    public T this[int i]
    {
        get => GetAt(i);
        set => SetAt(i, value);
    }

    public T this[int row, int col]
    {
        get => GetAt(row, col);
        set => SetAt(row, col, value);
    }

    public T this[int index, IndexingMode mode]
    {
        get => GetAt(index, mode);
        set
        {

        }
    }

    public T GetAt(int index)
    {
        ValidateIndex(index);
        return Value[index];
    }

    public T GetAt(int index, IndexingMode mode)
    {
        ValidateIndex(index);
        if (mode.HasFlag(IndexingMode.SyncCPU))
            SyncCPU();
        return Value[index];
    }

    public T GetAt(int row, int col)
    {
        var computedIndex = GetIndexFromCoordinates(row, col);
        ValidateIndex(computedIndex);
        return Value[computedIndex];
    }

    public T GetAt(int index, int col, IndexingMode mode)
    {
        var computedIndex = GetIndexFromCoordinates(index, col);
        ValidateIndex(computedIndex);
        if (mode.HasFlag(IndexingMode.SyncCPU))
            SyncCPU();
        return Value[computedIndex];
    }

    public void SetAt(int index, T val)
    {
        ValidateIndex(index);
        Value[index] = val;
    }

    public void SetAt(int index, IndexingMode mode, T val)
    {
        ValidateIndex(index);
        if (mode.HasFlag(IndexingMode.SyncCPU))
            SyncCPU();
        Value[index] = val;
        if (mode.HasFlag(IndexingMode.SyncGPU))
            UpdateCache();
    }

    public void SetAt(int row, int col, T val)
    {
        var computedIndex = GetIndexFromCoordinates(row, col);
        Value[computedIndex] = val;
    }

    public void SetAt(int index, int col, IndexingMode mode, T val)
    {
        var computedIndex = GetIndexFromCoordinates(index, col);
        if (mode.HasFlag(IndexingMode.SyncCPU))
            SyncCPU();
        Value[computedIndex] = val;
        if (mode.HasFlag(IndexingMode.SyncGPU))
            UpdateCache();
    }
}
