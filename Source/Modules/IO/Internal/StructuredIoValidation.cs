using System;
using BAVCL.Core.Helpers;

namespace BAVCL.Modules.IO.Internal;

internal static class StructuredIoValidation
{
	internal const int CurrentSchemaVersion = 1;

	internal static void ValidateSchemaVersion(int schemaVersion)
	{
		if (schemaVersion is not 0 and not CurrentSchemaVersion)
			throw new FormatException($"Unsupported schemaVersion {schemaVersion}. Supported: {CurrentSchemaVersion}.");
	}

	internal static void ValidateOptionalType(string? type, string expectedType)
	{
		if (type is null)
			return;

		if (!string.Equals(type, expectedType, StringComparison.Ordinal))
			throw new FormatException($"Document type '{type}' does not match expected '{expectedType}'.");
	}

	internal static void ValidateOptionalType(string? type, Type expectedType) =>
		ValidateOptionalType(type, IoSchema.Document.Of(expectedType));

	internal static void ValidateOptionalDtype(string? dtype, string expectedDtype)
	{
		if (dtype is null)
			return;

		if (!string.Equals(dtype, expectedDtype, StringComparison.Ordinal))
			throw new FormatException($"Document dtype '{dtype}' does not match expected '{expectedDtype}'.");
	}

	internal static void ValidateOptionalDtype(string? dtype, Type expectedElementType) =>
		ValidateOptionalDtype(dtype, IoSchema.Dtype.Of(expectedElementType));

	internal static void ValidateColumns(int columns)
	{
		if (columns < 0)
			throw new FormatException($"Document '{IoSchema.Field.Columns}' must be >= 0. Received {columns}.");
	}

	internal static void ValidateVector3Layout(int columns, int dataLength)
	{
		if (columns != 3)
			throw new FormatException($"Vector3 'columns' must be 3. Received {columns}.");

		if (dataLength % 3 != 0)
			throw new FormatException($"Vector3 length {dataLength} is not a multiple of 3.");
	}

	internal static void ValidateMaskBoolLayout(int columns, int dataLength)
	{
		ValidateColumns(columns);

		if (dataLength == 0)
			throw new FormatException($"Mask bool '{IoSchema.Field.Data}' must contain at least one element.");
	}

	internal static void ValidateMaskPackedLayout(int columns, int count, int wordLength)
	{
		ValidateColumns(columns);

		if (count <= 0)
			throw new FormatException($"Mask packed '{IoSchema.Field.Count}' must be > 0. Received {count}.");

		int expectedWords = MaskBitOps.WordCount(count);
		if (wordLength != expectedWords)
			throw new FormatException(
				$"Mask packed word length {wordLength} does not match count {count} (expected {expectedWords} words).");
	}
}
