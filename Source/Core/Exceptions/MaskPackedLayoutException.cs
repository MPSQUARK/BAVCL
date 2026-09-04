using System;

namespace BAVCL.Core.Exceptions;

/// <summary>Thrown when packed mask word storage does not match the logical element count.</summary>
public sealed class MaskPackedLayoutException(int elementCount, int wordCount, int expectedWordCount)
	: Exception($"Packed word count {wordCount} does not match element count {elementCount} (expected {expectedWordCount} words).")
{
	public int ElementCount { get; } = elementCount;

	public int WordCount { get; } = wordCount;

	public int ExpectedWordCount { get; } = expectedWordCount;
}
