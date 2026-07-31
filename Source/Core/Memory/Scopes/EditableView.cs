using System;

namespace BAVCL.Core;

public readonly ref struct EditableView<T> where T : unmanaged
{
	readonly Span<T> _span;

	internal EditableView(Memory<T> memory) => _span = memory.Span;

	public T this[int index]
	{
		get
		{
			ValidateIndex(index);
			return _span[index];
		}
		set
		{
			ValidateIndex(index);
			_span[index] = value;
		}
	}

	void ValidateIndex(int index)
	{
		if ((uint)index >= (uint)_span.Length)
			throw new IndexOutOfRangeException(
				$"Index {index} is out of range for span of length {_span.Length}.");
	}
}
