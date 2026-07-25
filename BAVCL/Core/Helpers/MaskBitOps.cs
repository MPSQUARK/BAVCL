using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BAVCL.Core.Helpers;

// bool[] <-> packed int32[] for Mask storage (32 bools per word, LSB-first).
// Pack/Unpack batch storage words via Vector<int>.Count; tails handle partial words.
// CompressWord folds 32 bool bytes into one int; FoldFourBools is the only parallel fold.
internal static class MaskBitOps
{
	internal const int BitsPerWord = 32;
	internal const int WordShift = 5;
	internal const int WordMask = 31;

	const uint BoolByteLaneMask = 0x01010101;
	const uint FoldFourBoolsMagic = 0x01020408;

	internal static int WordCount(int elementCount)
	{
		if (elementCount < 0)
			throw new ArgumentOutOfRangeException(nameof(elementCount), elementCount, "Element count must be zero or greater.");

		return elementCount == 0 ? 0 : (elementCount + WordMask) >> WordShift;
	}

	internal static int[] Pack(ReadOnlySpan<bool> values)
	{
		int[] words = new int[WordCount(values.Length)];
		if (values.Length == 0)
			return words;

		int fullWordCount = values.Length >> WordShift;
		int batchWidth = Vector<int>.Count;
		Span<int> batchBuffer = stackalloc int[batchWidth];

		for (int batchStart = 0; batchStart + batchWidth <= fullWordCount; batchStart += batchWidth)
		{
			for (int lane = 0; lane < batchWidth; lane++)
				batchBuffer[lane] = CompressWord(values, (batchStart + lane) << WordShift);

			new Vector<int>(batchBuffer).CopyTo(words.AsSpan(batchStart, batchWidth));
		}

		int partialBatchStart = (fullWordCount / batchWidth) * batchWidth;
		for (int wordIndex = partialBatchStart; wordIndex < fullWordCount; wordIndex++)
			words[wordIndex] = CompressWord(values, wordIndex << WordShift);

		int remainder = values.Length & WordMask;
		if (remainder == 0)
			return words;

		int tailStart = fullWordCount << WordShift;
		words[fullWordCount] = CompressWord(values, tailStart, remainder);
		words[fullWordCount] &= (1 << remainder) - 1;

		return words;
	}

	internal static bool[] Unpack(ReadOnlySpan<int> words, int elementCount)
	{
		bool[] values = new bool[elementCount];
		if (elementCount == 0)
			return values;

		int fullWordCount = elementCount >> WordShift;
		int batchWidth = Vector<int>.Count;

		for (int batchStart = 0; batchStart + batchWidth <= fullWordCount; batchStart += batchWidth)
		{
			Vector<int> batch = new(words.Slice(batchStart, batchWidth));
			for (int lane = 0; lane < batchWidth; lane++)
				ExpandWord(batch[lane], values.AsSpan((batchStart + lane) << WordShift, BitsPerWord));
		}

		int partialBatchStart = (fullWordCount / batchWidth) * batchWidth;
		for (int wordIndex = partialBatchStart; wordIndex < fullWordCount; wordIndex++)
			ExpandWord(words[wordIndex], values.AsSpan(wordIndex << WordShift, BitsPerWord));

		int remainder = elementCount & WordMask;
		if (remainder == 0)
			return values;

		ExpandWord(words[fullWordCount], values.AsSpan(fullWordCount << WordShift, remainder), remainder);
		return values;
	}

	internal static bool GetBit(ReadOnlySpan<int> words, int index)
	{
		int bitIndex = index & WordMask;
		return (words[index >> WordShift] & (1 << bitIndex)) != 0;
	}

	internal static int ApplyBit(int word, int bitIndex, bool value)
	{
		int bitMask = 1 << bitIndex;
		return (word & ~bitMask) | (BitAsInt(value) << bitIndex);
	}

	internal static void ClearPaddingBits(Span<int> words, int elementCount)
	{
		if (elementCount == 0 || words.Length == 0)
			return;

		int trailingBits = elementCount & WordMask;
		if (trailingBits == 0)
			return;

		words[^1] &= (1 << trailingBits) - 1;
	}

	static int CompressWord(ReadOnlySpan<bool> values, int startIndex, int count = BitsPerWord)
	{
		ref bool boolRef = ref MemoryMarshal.GetReference(values);
		ref byte start = ref Unsafe.As<bool, byte>(ref Unsafe.Add(ref boolRef, startIndex));

		int word = 0;
		int groupCount = (count + 3) >> 2;

		for (int group = 0; group < groupCount; group++)
		{
			int groupStart = group << 2;
			int bitsInGroup = Math.Min(4, count - groupStart);
			word |= FoldBoolBytes(ref Unsafe.Add(ref start, groupStart), bitsInGroup) << groupStart;
		}

		return word;
	}

	static int FoldBoolBytes(ref byte start, int count)
	{
		if (count == 4)
		{
			uint lanes = Unsafe.ReadUnaligned<uint>(ref start);
			return FoldFourBools(lanes);
		}

		Span<byte> buffer = stackalloc byte[4];
		for (int bit = 0; bit < count; bit++)
			buffer[bit] = Unsafe.Add(ref start, bit);

		return FoldFourBools(Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(buffer)));
	}

	static void ExpandWord(int word, Span<bool> destination, int count = BitsPerWord)
	{
		Span<byte> bytes = MemoryMarshal.AsBytes(destination[..count]);
		int groupCount = (count + 3) >> 2;

		for (int group = 0; group < groupCount; group++)
		{
			int groupStart = group << 2;
			int nibble = (word >> (group << 2)) & 0xF;
			int bitsInGroup = Math.Min(4, count - groupStart);

			for (int bit = 0; bit < bitsInGroup; bit++)
				bytes[groupStart + bit] = (byte)((nibble >> bit) & 1);
		}
	}

	// Four bool bytes (each 0 or 1) -> 4-bit nibble. Multiply/shift folds lanes in parallel.
	static int FoldFourBools(uint lanes)
	{
		lanes &= BoolByteLaneMask;
		return (int)((lanes * FoldFourBoolsMagic) >> 24);
	}

	static int BitAsInt(bool value) => Unsafe.As<bool, byte>(ref value);
}
