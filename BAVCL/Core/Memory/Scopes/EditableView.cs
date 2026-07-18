namespace BAVCL.Core;

public readonly ref struct EditableView<T> where T : unmanaged
{
	readonly VectorBase<T> _owner;

	internal EditableView(VectorBase<T> owner) => _owner = owner;

	public T this[int index]
	{
		get
		{
			_owner.ValidateIndexForView(index);
			return _owner.Value[index];
		}
		set
		{
			_owner.ValidateIndexForView(index);
			_owner.Value[index] = value;
		}
	}

	public T this[int row, int col]
	{
		get
		{
			int index = _owner.GetIndexFromCoordinatesForView(row, col);
			return this[index];
		}
		set
		{
			int index = _owner.GetIndexFromCoordinatesForView(row, col);
			this[index] = value;
		}
	}
}
