namespace BAVCL.Core;

public abstract partial class VectorBase<T>
{
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

    public T GetAt(int index)
    {
        ValidateIndexForView(index);
        return RetrieveReadOnlySpan()[index];
    }

    public T GetAt(int row, int col)
    {
        int computedIndex = GetIndexFromCoordinatesForView(row, col);
        return RetrieveReadOnlySpan()[computedIndex];
    }

    public void SetAt(int index, T val)
    {
        ValidateIndexForView(index);
        using (var scope = CpuScope(syncOnDispose: false))
        {
            EditableView<T> view = scope.View;
            view[index] = val;
        }
    }

    public void SetAt(int row, int col, T val)
    {
        int computedIndex = GetIndexFromCoordinatesForView(row, col);
        ValidateIndexForView(computedIndex);
        using (var scope = CpuScope(syncOnDispose: false))
        {
            EditableView<T> view = scope.View;
            view[computedIndex] = val;
        }
    }
}
