using System;
using ILGPU.Runtime;
using BAVCL.Core.Exceptions;
using BAVCL.Core.Helpers;
using BAVCL.Modules.Masking;

namespace BAVCL.Types;

/// <summary>
/// Packed boolean mask stored as densely packed int32 words (32 booleans per word, LSB-first).
/// <see cref="ElementCount"/> is the logical boolean count and matches <see cref="Vector.Length"/>.
/// Inherited <see cref="CacheableBase{T}.Length"/> and <see cref="RetrieveReadOnlySpan"/> use packed storage words;
/// use <see cref="ToBoolArray"/> for one bool per logical element.
/// </summary>
public sealed class Mask : CacheableBase<int>
{
	readonly int _elementCount;
	int _columns;

	/// <summary>Logical boolean element count; matches <see cref="Vector.Length"/>.</summary>
	public int ElementCount => _elementCount;

	internal int WordCount => _length;

	public int Columns
	{
		get => _columns;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(Columns), value, "Columns must be zero or greater.");

			_columns = value;
		}
	}

	public bool this[int index]
	{
		get => GetBit(index);
		set => SetBit(index, value);
	}

	public Mask(GPU gpu, bool[] values, int columns = 0, bool cache = true)
		: this(gpu, values.AsSpan(), columns, cache)
	{ }

	public Mask(GPU gpu, ReadOnlySpan<bool> values, int columns = 0, bool cache = true)
		: base(gpu, MaskBitOps.Pack(ValidateValues(values)), cache)
	{
		_elementCount = values.Length;
		_columns = columns;
		ValidateRectangularLayout();
	}

	public Mask(GPU gpu, int elementCount, int columns = 0, bool cache = true)
		: base(gpu, new int[MaskBitOps.WordCount(ValidateElementCount(elementCount))], cache)
	{
		_elementCount = elementCount;
		_columns = columns;
		ValidateRectangularLayout();
	}

	public Mask(GPU gpu, int[] words, int elementCount, int columns = 0, bool cache = true)
		: base(gpu, PrepareWords(words, elementCount), cache)
	{
		_elementCount = elementCount;
		_columns = columns;
		ValidateRectangularLayout();
	}

	private Mask(GPU gpu, int elementCount, int columns) : base(gpu, [], cache: false)
	{
		_elementCount = elementCount;
		_columns = columns;
		ValidateRectangularLayout();
	}

	/// <summary>Creates an empty vessel shell with no GPU buffer.</summary>
	public static Vessel<Mask> CreateVessel(GPU gpu, int elementCount, int columns = 0) =>
		new(new Mask(gpu, elementCount, columns), gpu);

	public int RowCount()
	{
		if (Columns == 0)
			return 1;

		if (Columns == 1)
			return _elementCount;

		return _elementCount / Columns;
	}

	public Shape Shape() => BAVCL.Shape.FromStorage(_elementCount, Columns);

	public bool IsRectangular() => Columns == 0 || _elementCount % Columns == 0;

	public bool Is1D() => Columns == 0;

	public bool GetBit(int index)
	{
		ValidateIndex(index);
		ReadOnlySpan<int> words = RetrieveReadOnlySpan();
		return MaskBitOps.GetBit(words, index);
	}

	public void SetBit(int index, bool value)
	{
		ValidateIndex(index);
		int wordIndex = index >> MaskBitOps.WordShift;
		int bitIndex = index & MaskBitOps.WordMask;

		using (var scope = this.CpuScope())
		{
			EditableView<int> view = scope.View;
			view[wordIndex] = MaskBitOps.ApplyBit(view[wordIndex], bitIndex, value);
		}
	}

	/// <summary>
	/// Unpacks one bool per logical element (<see cref="ElementCount"/>).
	/// Intended for debugging and interop — not for hot paths; prefer <see cref="GetBit"/>,
	/// the indexer, or packed <see cref="RetrieveReadOnlySpan"/> / GPU buffers instead.
	/// </summary>
	public bool[] ToBoolArray()
	{
		ReadOnlySpan<int> words = RetrieveReadOnlySpan();
		return MaskBitOps.Unpack(words, _elementCount);
	}

	/// <summary>Returns packed int32 storage words (<see cref="WordCount"/> elements).</summary>
	public int[] ToWordArray() => base.ToArray();

	public new MemoryBuffer UpdateCache(int[] array) =>
		base.UpdateCache(PrepareWords(array, _elementCount));

	public void ValidateShapeMatches(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		ValidateShapeMatches(vector.Shape(), vector.Length);
	}

	public void ValidateShapeMatches(Shape shape, int elementCount)
	{
		if (_elementCount != elementCount)
			throw new ShapeMismatchException(
				"mask validation",
				$"mask elementCount={_elementCount}",
				$"elementCount={elementCount}");

		if (!Shape().MatchesDimensions(shape))
			throw new ShapeMismatchException("mask validation", Shape(), shape);
	}

	private static ReadOnlySpan<bool> ValidateValues(ReadOnlySpan<bool> values)
	{
		if (values.Length == 0)
			throw new ArgumentException("Mask must contain at least one boolean element.", nameof(values));

		return values;
	}

	private static int ValidateElementCount(int elementCount)
	{
		if (elementCount <= 0)
			throw new ArgumentOutOfRangeException(nameof(elementCount), elementCount, "Element count must be greater than zero.");

		return elementCount;
	}

	private static int[] PrepareWords(int[] words, int elementCount)
	{
		ValidateElementCount(elementCount);
		ArgumentNullException.ThrowIfNull(words);

		int expectedWordCount = MaskBitOps.WordCount(elementCount);
		if (words.Length != expectedWordCount)
			throw new MaskPackedLayoutException(elementCount, words.Length, expectedWordCount);

		int[] storage = (int[])words.Clone();
		MaskBitOps.ClearPaddingBits(storage, elementCount);
		return storage;
	}

	public void ValidateIndex(int index)
	{
		if (index < 0 || index >= _elementCount)
			throw new IndexOutOfRangeException($"Index {index} is out of range for mask of length {_elementCount}.");
	}

	private void ValidateRectangularLayout()
	{
		if (!IsRectangular())
			throw new ShapeMismatchException(
				"mask construction",
				$"elementCount={_elementCount}, columns={Columns}",
				"(invalid storage layout)");
	}

	public static Mask operator &(Mask left, Mask right) =>
		MaskBitwiseOps.BinaryOp(left, right, MaskOperation.And);

	public static Mask operator |(Mask left, Mask right) =>
		MaskBitwiseOps.BinaryOp(left, right, MaskOperation.Or);

	public static Mask operator ^(Mask left, Mask right) =>
		MaskBitwiseOps.BinaryOp(left, right, MaskOperation.Xor);

	public static Mask operator ~(Mask mask) => MaskBitwiseOps.Complement(mask);

	public static Mask operator !(Mask mask) => MaskBitwiseOps.Complement(mask);

	/// <summary>Mask of the same layout with every lane set.</summary>
	public static Mask operator +(Mask mask) => MaskBitwiseOps.AllSet(mask);

	/// <summary>Mask of the same layout with every lane clear.</summary>
	public static Mask operator -(Mask mask) => MaskBitwiseOps.AllClear(mask);

	public void operator &=(Mask other) => MaskBitwiseOps.BinaryOpInPlace(this, other, MaskOperation.And);

	public void operator |=(Mask other) => MaskBitwiseOps.BinaryOpInPlace(this, other, MaskOperation.Or);

	public void operator ^=(Mask other) => MaskBitwiseOps.BinaryOpInPlace(this, other, MaskOperation.Xor);
}
