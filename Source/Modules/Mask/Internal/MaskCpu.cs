using System;
using System.Numerics;
using BAVCL.Core.Helpers;
using BAVCL.Types;

namespace BAVCL.Modules.Masking;

/// <summary>CPU mask word analytics: popcount, any/all, first/last true index.</summary>
internal static class MaskCpu
{
	internal static int CountTrue(Mask mask)
	{
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();
		int elementCount = mask.ElementCount;
		if (elementCount == 0)
			return 0;

		int count = 0;
		int lastWord = words.Length - 1;
		int tail = MaskBitOps.TailMask(elementCount);

		for (int wordIndex = 0; wordIndex < lastWord; wordIndex++)
			count += BitOperations.PopCount((uint)words[wordIndex]);

		count += BitOperations.PopCount((uint)(words[lastWord] & tail));
		return count;
	}

	internal static bool Any(Mask mask)
	{
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();
		int elementCount = mask.ElementCount;
		if (elementCount == 0)
			return false;

		int lastWord = words.Length - 1;
		int tail = MaskBitOps.TailMask(elementCount);

		for (int wordIndex = 0; wordIndex < lastWord; wordIndex++)
		{
			if (words[wordIndex] != 0)
				return true;
		}

		return (words[lastWord] & tail) != 0;
	}

	internal static bool All(Mask mask) =>
		MaskBitOps.AreAllLogicalBitsSet(mask.ElementCount, mask.RetrieveReadOnlySpan());

	internal static int FirstTrueIndex(Mask mask) => ScanForward(mask);

	internal static int LastTrueIndex(Mask mask) => ScanBackward(mask);

	static int ScanForward(Mask mask)
	{
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();
		int elementCount = mask.ElementCount;
		if (elementCount == 0)
			return -1;

		int tail = MaskBitOps.TailMask(elementCount);

		for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
		{
			int word = words[wordIndex];
			if (wordIndex == words.Length - 1)
				word &= tail;

			if (word == 0)
				continue;

			return wordIndex * MaskBitOps.BitsPerWord + BitOperations.TrailingZeroCount((uint)word);
		}

		return -1;
	}

	static int ScanBackward(Mask mask)
	{
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();
		int elementCount = mask.ElementCount;
		if (elementCount == 0)
			return -1;

		int tail = MaskBitOps.TailMask(elementCount);

		for (int wordIndex = words.Length - 1; wordIndex >= 0; wordIndex--)
		{
			int word = words[wordIndex];
			if (wordIndex == words.Length - 1)
				word &= tail;

			if (word == 0)
				continue;

			int bit = MaskBitOps.BitsPerWord - 1 - BitOperations.LeadingZeroCount((uint)word);
			return wordIndex * MaskBitOps.BitsPerWord + bit;
		}

		return -1;
	}
}
