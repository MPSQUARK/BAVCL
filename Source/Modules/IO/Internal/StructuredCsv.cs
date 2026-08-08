using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BAVCL.Core.Helpers;
using BAVCL.Modules.IO.Internal.Schema;
using BAVCL.Types;

namespace BAVCL.Modules.IO.Internal;

internal static class StructuredCsv
{
	const char DataSeparator = ';';

	internal static string SerializeFloatArray(Type type, int columns, ReadOnlySpan<float> data) =>
		$"{string.Join(',', IoSchema.Field.DefaultHeader)}{Environment.NewLine}{SerializeFloatArrayFragmentRow(type, columns, data)}";

	internal static string SerializeFloatArrayFragmentRow(Type type, int columns, ReadOnlySpan<float> data)
	{
		string dataField = JoinDataField(data, FloatIoParsing.FormatFloat);
		return string.Join(',', BuildFragmentMetadataValues(type, typeof(float), columns, dataField));
	}

	internal static string OpenFloatArrayCollection(Type type, int columns, ReadOnlySpan<float> firstItem) =>
		$"{IoSchema.Collection.CsvSchemaVersionLine(StructuredIoValidation.CurrentSchemaVersion)}{Environment.NewLine}{string.Join(',', IoSchema.Field.ItemDefaultHeader)}{Environment.NewLine}{SerializeFloatArrayItemRow(type, columns, firstItem)}";

	internal static string SerializeFloatArrayItemRow(Type type, int columns, ReadOnlySpan<float> data)
	{
		string dataField = JoinDataField(data, FloatIoParsing.FormatFloat);
		return string.Join(',', BuildItemMetadataValues(type, typeof(float), columns, dataField));
	}

	internal static string SerializeIntArray(Type type, int columns, ReadOnlySpan<int> data) =>
		$"{string.Join(',', IoSchema.Field.DefaultHeader)}{Environment.NewLine}{SerializeIntArrayFragmentRow(type, columns, data)}";

	internal static string SerializeIntArrayFragmentRow(Type type, int columns, ReadOnlySpan<int> data)
	{
		string dataField = JoinDataField(data, IntIoParsing.FormatInt);
		return string.Join(',', BuildFragmentMetadataValues(type, typeof(int), columns, dataField));
	}

	internal static string OpenIntArrayCollection(Type type, int columns, ReadOnlySpan<int> firstItem) =>
		$"{IoSchema.Collection.CsvSchemaVersionLine(StructuredIoValidation.CurrentSchemaVersion)}{Environment.NewLine}{string.Join(',', IoSchema.Field.ItemDefaultHeader)}{Environment.NewLine}{SerializeIntArrayItemRow(type, columns, firstItem)}";

	internal static string SerializeIntArrayItemRow(Type type, int columns, ReadOnlySpan<int> data)
	{
		string dataField = JoinDataField(data, IntIoParsing.FormatInt);
		return string.Join(',', BuildItemMetadataValues(type, typeof(int), columns, dataField));
	}

	internal static IntArrayDocument DeserializeIntArray(string csv, Type expectedType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		(string[] header, List<string[]> rows) = ParseRows(csv);
		if (rows.Count != 1)
			throw new FormatException($"CSV document must contain exactly one data row. Received {rows.Count}. Use DeserializeAllIntArray for multi-row files.");

		return BuildIntArrayDocument(ZipFields(header, rows[0]), expectedType);
	}

	internal static IReadOnlyList<IntArrayDocument> DeserializeAllIntArray(string csv, Type expectedType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		(int schemaVersion, string[] header, List<string[]> rows) = ParseCollectionFile(csv);
		StructuredIoValidation.ValidateSchemaVersion(schemaVersion);
		return rows.Select(row => BuildIntArrayDocumentFromItem(ZipFields(header, row), expectedType, schemaVersion)).ToList();
	}

	static IntArrayDocument BuildIntArrayDocumentFromItem(
		IReadOnlyDictionary<string, string> fields,
		Type expectedType,
		int schemaVersion)
	{
		var document = new IntArrayDocument
		{
			SchemaVersion = schemaVersion,
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Data = ParseIntArrayData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateOptionalType(document.Type, expectedType);
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(int));
		StructuredIoValidation.ValidateColumns(document.Columns);

		return document;
	}

	static IntArrayDocument BuildIntArrayDocument(IReadOnlyDictionary<string, string> fields, Type expectedType)
	{
		var document = new IntArrayDocument
		{
			SchemaVersion = ReadIntField(fields, IoSchema.Field.SchemaVersion),
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Data = ParseIntArrayData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion);
		StructuredIoValidation.ValidateOptionalType(document.Type, expectedType);
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(int));
		StructuredIoValidation.ValidateColumns(document.Columns);

		return document;
	}

	internal static string SerializeMaskBool(int columns, ReadOnlySpan<bool> data) =>
		$"{string.Join(',', IoSchema.Field.DefaultHeader)}{Environment.NewLine}{SerializeMaskBoolFragmentRow(columns, data)}";

	internal static string SerializeMaskBoolFragmentRow(int columns, ReadOnlySpan<bool> data)
	{
		string dataField = JoinDataField(data, BoolIoParsing.FormatCsv);
		return string.Join(',', BuildFragmentMetadataValues(typeof(Mask), typeof(bool), columns, dataField));
	}

	internal static string OpenMaskBoolCollection(int columns, ReadOnlySpan<bool> firstItem) =>
		$"{IoSchema.Collection.CsvSchemaVersionLine(StructuredIoValidation.CurrentSchemaVersion)}{Environment.NewLine}{string.Join(',', IoSchema.Field.ItemDefaultHeader)}{Environment.NewLine}{SerializeMaskBoolItemRow(columns, firstItem)}";

	internal static string SerializeMaskBoolItemRow(int columns, ReadOnlySpan<bool> data)
	{
		string dataField = JoinDataField(data, BoolIoParsing.FormatCsv);
		return string.Join(',', BuildItemMetadataValues(typeof(Mask), typeof(bool), columns, dataField));
	}

	internal static string SerializeMaskPacked(int columns, int count, ReadOnlySpan<int> words) =>
		$"{string.Join(',', IoSchema.Field.MaskPackedHeader)}{Environment.NewLine}{SerializeMaskPackedFragmentRow(columns, count, words)}";

	internal static string SerializeMaskPackedFragmentRow(int columns, int count, ReadOnlySpan<int> words)
	{
		string dataField = JoinDataField(words, static word => word.ToString(CultureInfo.InvariantCulture));
		return string.Join(',', BuildFragmentMetadataValues(typeof(Mask), typeof(int), columns, dataField, count));
	}

	internal static string OpenMaskPackedCollection(int columns, int count, ReadOnlySpan<int> firstWords) =>
		$"{IoSchema.Collection.CsvSchemaVersionLine(StructuredIoValidation.CurrentSchemaVersion)}{Environment.NewLine}{string.Join(',', IoSchema.Field.ItemMaskPackedHeader)}{Environment.NewLine}{SerializeMaskPackedItemRow(columns, count, firstWords)}";

	internal static string SerializeMaskPackedItemRow(int columns, int count, ReadOnlySpan<int> words)
	{
		string dataField = JoinDataField(words, static word => word.ToString(CultureInfo.InvariantCulture));
		return string.Join(',', BuildItemMetadataValues(typeof(Mask), typeof(int), columns, dataField, count));
	}

	internal static FloatArrayDocument DeserializeFloatArray(string csv, Type expectedType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		(string[] header, List<string[]> rows) = ParseRows(csv);
		if (rows.Count != 1)
			throw new FormatException($"CSV document must contain exactly one data row. Received {rows.Count}. Use DeserializeAllFloatArray for multi-row files.");

		return BuildFloatArrayDocument(ZipFields(header, rows[0]), expectedType);
	}

	internal static IReadOnlyList<FloatArrayDocument> DeserializeAllFloatArray(string csv, Type expectedType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		(int schemaVersion, string[] header, List<string[]> rows) = ParseCollectionFile(csv);
		StructuredIoValidation.ValidateSchemaVersion(schemaVersion);
		return rows.Select(row => BuildFloatArrayDocumentFromItem(ZipFields(header, row), expectedType, schemaVersion)).ToList();
	}

	internal static Mask DeserializeMask(GPU gpu, string csv)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		(string[] header, List<string[]> rows) = ParseRows(csv);
		if (rows.Count != 1)
			throw new FormatException($"CSV document must contain exactly one data row. Received {rows.Count}. Use DeserializeAllMask for multi-row files.");

		return DeserializeMaskRow(gpu, ZipFields(header, rows[0]));
	}

	internal static IReadOnlyList<Mask> DeserializeAllMask(GPU gpu, string csv)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		(int schemaVersion, string[] header, List<string[]> rows) = ParseCollectionFile(csv);
		StructuredIoValidation.ValidateSchemaVersion(schemaVersion);
		return rows.Select(row => DeserializeMaskItemRow(gpu, ZipFields(header, row), schemaVersion)).ToList();
	}

	static Mask DeserializeMaskRow(GPU gpu, IReadOnlyDictionary<string, string> fields)
	{
		int schemaVersion = ReadIntField(fields, IoSchema.Field.SchemaVersion);
		StructuredIoValidation.ValidateSchemaVersion(schemaVersion);
		return DeserializeMaskItemRow(gpu, fields, schemaVersion);
	}

	static Mask DeserializeMaskItemRow(GPU gpu, IReadOnlyDictionary<string, string> fields, int schemaVersion)
	{
		string? dtype = ReadOptionalStringField(fields, IoSchema.Field.Dtype);

		if (!MaskIoRouting.TryResolve(dtype, out MaskWireFormat format))
			throw new FormatException(MaskIoRouting.UnsupportedDtypeMessage(dtype, "CSV"));

		return format switch
		{
			MaskWireFormat.Packed => DeserializeMaskPackedItemRow(gpu, fields, schemaVersion),
			MaskWireFormat.Bool => DeserializeMaskBoolItemRow(gpu, fields, schemaVersion),
			_ => throw new InvalidOperationException($"Unsupported mask wire format '{format}'."),
		};
	}

	static FloatArrayDocument BuildFloatArrayDocumentFromItem(
		IReadOnlyDictionary<string, string> fields,
		Type expectedType,
		int schemaVersion)
	{
		var document = new FloatArrayDocument
		{
			SchemaVersion = schemaVersion,
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Data = ParseFloatData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateOptionalType(document.Type, expectedType);
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(float));
		StructuredIoValidation.ValidateColumns(document.Columns);

		return document;
	}

	static Mask DeserializeMaskBoolItemRow(GPU gpu, IReadOnlyDictionary<string, string> fields, int schemaVersion)
	{
		var document = new MaskBoolDocument
		{
			SchemaVersion = schemaVersion,
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Data = ParseBoolData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask));
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(bool));
		StructuredIoValidation.ValidateMaskBoolLayout(document.Columns, document.Data.Length);

		return new Mask(gpu, document.Data, document.Columns);
	}

	static Mask DeserializeMaskPackedItemRow(GPU gpu, IReadOnlyDictionary<string, string> fields, int schemaVersion)
	{
		var document = new MaskPackedDocument
		{
			SchemaVersion = schemaVersion,
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Count = ReadIntField(fields, IoSchema.Field.Count),
			Data = ParseIntData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask));
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(int));
		StructuredIoValidation.ValidateMaskPackedLayout(document.Columns, document.Count, document.Data.Length);

		return new Mask(gpu, document.Data, document.Count, document.Columns);
	}

	static FloatArrayDocument BuildFloatArrayDocument(IReadOnlyDictionary<string, string> fields, Type expectedType)
	{
		var document = new FloatArrayDocument
		{
			SchemaVersion = ReadIntField(fields, IoSchema.Field.SchemaVersion),
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Data = ParseFloatData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion);
		StructuredIoValidation.ValidateOptionalType(document.Type, expectedType);
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(float));
		StructuredIoValidation.ValidateColumns(document.Columns);

		return document;
	}

	static string[] BuildItemMetadataValues(Type documentType, Type elementType, int columns, string dataField, int? count = null)
	{
		var values = new List<string>(count is int ? 5 : 4)
		{
			IoSchema.Document.Of(documentType),
			IoSchema.Dtype.Of(elementType),
			columns.ToString(CultureInfo.InvariantCulture),
		};

		if (count is int packedCount)
			values.Add(packedCount.ToString(CultureInfo.InvariantCulture));

		values.Add(dataField);
		return values.ToArray();
	}

	static string[] BuildFragmentMetadataValues(Type documentType, Type elementType, int columns, string dataField, int? count = null)
	{
		var values = new List<string>(count is int ? 6 : 5)
		{
			StructuredIoValidation.CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture),
			IoSchema.Document.Of(documentType),
			IoSchema.Dtype.Of(elementType),
			columns.ToString(CultureInfo.InvariantCulture),
		};

		if (count is int packedCount)
			values.Add(packedCount.ToString(CultureInfo.InvariantCulture));

		values.Add(dataField);
		return values.ToArray();
	}

	/// <summary>
	/// Parses a collection CSV file: schema-version line, item header row, then one or more data rows.
	/// </summary>
	static (int SchemaVersion, string[] Header, List<string[]> Rows) ParseCollectionFile(string csv)
	{
		string[] lines = csv
			.Replace("\r\n", "\n")
			.Replace('\r', '\n')
			.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (lines.Length < 3)
			throw new FormatException($"CSV collection must contain a schema-version line, a header row, and at least one data row. Received {lines.Length} non-empty lines.");

		string[] schemaLine = lines[0].Split(',');
		if (schemaLine.Length != 2
			|| !string.Equals(schemaLine[0].Trim(), IoSchema.Field.SchemaVersion, StringComparison.OrdinalIgnoreCase))
			throw new FormatException($"CSV collection must start with a '{IoSchema.Field.SchemaVersion},<version>' line.");

		if (!int.TryParse(schemaLine[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int schemaVersion))
			throw new FormatException($"CSV collection schema version '{schemaLine[1]}' is not a valid integer.");

		string[] header = lines[1].Split(',');
		if (!string.Equals(header[^1], IoSchema.Field.Data, StringComparison.OrdinalIgnoreCase))
			throw new FormatException($"CSV header must end with a '{IoSchema.Field.Data}' column.");

		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string name in header)
		{
			string trimmed = name.Trim();
			if (trimmed.Length == 0)
				throw new FormatException("CSV header contains an empty column name.");

			if (!seen.Add(trimmed))
				throw new FormatException($"CSV header contains duplicate column '{trimmed}'.");
		}

		var rows = new List<string[]>(lines.Length - 2);
		for (int i = 2; i < lines.Length; i++)
		{
			string[] values = lines[i].Split(',');
			if (values.Length != header.Length)
				throw new FormatException($"CSV row {i} column count {values.Length} does not match header column count {header.Length}.");

			rows.Add(values);
		}

		return (schemaVersion, header, rows);
	}

	/// <summary>
	/// Parses a single-document CSV fragment into its header and one data row.
	/// </summary>
	static (string[] Header, List<string[]> Rows) ParseRows(string csv)
	{
		string[] lines = csv
			.Replace("\r\n", "\n")
			.Replace('\r', '\n')
			.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (lines.Length < 2)
			throw new FormatException($"CSV document must contain a header row and at least one data row. Received {lines.Length} non-empty lines.");

		string[] header = lines[0].Split(',');

		if (!string.Equals(header[^1], IoSchema.Field.Data, StringComparison.OrdinalIgnoreCase))
			throw new FormatException($"CSV header must end with a '{IoSchema.Field.Data}' column.");

		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string name in header)
		{
			string trimmed = name.Trim();
			if (trimmed.Length == 0)
				throw new FormatException("CSV header contains an empty column name.");

			if (!seen.Add(trimmed))
				throw new FormatException($"CSV header contains duplicate column '{trimmed}'.");
		}

		var rows = new List<string[]>(lines.Length - 1);
		for (int i = 1; i < lines.Length; i++)
		{
			string[] values = lines[i].Split(',');
			if (values.Length != header.Length)
				throw new FormatException($"CSV row {i} column count {values.Length} does not match header column count {header.Length}.");

			rows.Add(values);
		}

		return (header, rows);
	}

	static IReadOnlyDictionary<string, string> ZipFields(string[] header, string[] values)
	{
		var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < header.Length; i++)
			fields[header[i].Trim()] = values[i].Trim();

		return fields;
	}

	static string JoinDataField<TValue>(ReadOnlySpan<TValue> values, Func<TValue, string> format)
	{
		if (values.Length == 0)
			return string.Empty;

		var field = new StringBuilder();
		for (int i = 0; i < values.Length; i++)
		{
			if (i > 0)
				field.Append(DataSeparator);

			field.Append(format(values[i]));
		}

		return field.ToString();
	}

	static float[] ParseFloatData(string dataField)
	{
		if (dataField.Length == 0)
			return [];

		return SplitDataField(dataField)
			.Select(token =>
			{
				try
				{
					return FloatIoParsing.ParseFloat(token);
				}
				catch (Exception ex) when (ex is FormatException or OverflowException)
				{
					throw new FormatException($"CSV data contains invalid float '{token}'.", ex);
				}
			})
			.ToArray();
	}

	static int[] ParseIntArrayData(string dataField)
	{
		if (dataField.Length == 0)
			return [];

		return SplitDataField(dataField)
			.Select(token =>
			{
				try
				{
					return IntIoParsing.ParseInt(token);
				}
				catch (FormatException ex)
				{
					throw new FormatException($"CSV data contains invalid int32 '{token}'.", ex);
				}
			})
			.ToArray();
	}

	static bool[] ParseBoolData(string dataField)
	{
		if (dataField.Length == 0)
			throw new FormatException($"CSV mask '{IoSchema.Field.Data}' must contain at least one element.");

		return SplitDataField(dataField)
			.Select(BoolIoParsing.ParseWireToken)
			.ToArray();
	}

	static int[] ParseIntData(string dataField)
	{
		if (dataField.Length == 0)
			throw new FormatException($"CSV packed mask '{IoSchema.Field.Data}' must contain at least one word.");

		return SplitDataField(dataField)
			.Select(token =>
			{
				if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
					throw new FormatException($"CSV packed mask data contains invalid integer '{token}'.");

				return value;
			})
			.ToArray();
	}

	static string[] SplitDataField(string dataField)
	{
		if (dataField.Length == 0)
			return [];

		string[] tokens = dataField.Split(DataSeparator);
		for (int i = 0; i < tokens.Length; i++)
		{
			string token = tokens[i].Trim();
			if (token.Length == 0)
				throw new FormatException($"CSV '{IoSchema.Field.Data}' contains an empty element.");

			tokens[i] = token;
		}

		return tokens;
	}

	static int ReadIntField(IReadOnlyDictionary<string, string> fields, string name)
	{
		string value = ReadStringField(fields, name);
		if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
			throw new FormatException($"CSV field '{name}' value '{value}' is not a valid integer.");

		return parsed;
	}

	static string ReadStringField(IReadOnlyDictionary<string, string> fields, string name) =>
		ReadOptionalStringField(fields, name)
		?? throw new FormatException($"CSV field '{name}' is required.");

	static string? ReadOptionalStringField(IReadOnlyDictionary<string, string> fields, string name) =>
		fields.TryGetValue(name, out string? value) ? value : null;
}
