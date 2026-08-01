using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BAVCL.Core.Helpers;
using BAVCL.Modules.IO.Internal.Schema;
using BAVCL.Types;

namespace BAVCL.Modules.IO.Internal;

internal static class StructuredCsv
{
	const char DataSeparator = ';';

	internal static string SerializeFloatArray(Type type, int columns, float[] data)
	{
		string dataField = JoinDataField(data.Select(FloatIoParsing.FormatFloat));
		return BuildDocument(
			IoSchema.Field.DefaultHeader,
			BuildMetadataValues(type, typeof(float), columns, dataField));
	}

	internal static string SerializeMaskBool(int columns, bool[] data)
	{
		string dataField = JoinDataField(data.Select(BoolIoParsing.FormatCsv));
		return BuildDocument(
			IoSchema.Field.DefaultHeader,
			BuildMetadataValues(typeof(Mask), typeof(bool), columns, dataField));
	}

	internal static string SerializeMaskPacked(int columns, int count, int[] words)
	{
		string dataField = JoinDataField(words.Select(word => word.ToString(CultureInfo.InvariantCulture)));
		return BuildDocument(
			IoSchema.Field.MaskPackedHeader,
			BuildMetadataValues(typeof(Mask), typeof(int), columns, dataField, count));
	}

	internal static FloatArrayDocument DeserializeFloatArray(string csv, Type expectedType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		IReadOnlyDictionary<string, string> fields = ParseDocument(csv);
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

	internal static Mask DeserializeMask(GPU gpu, string csv)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		IReadOnlyDictionary<string, string> fields = ParseDocument(csv);
		string? dtype = ReadOptionalStringField(fields, IoSchema.Field.Dtype);

		if (!MaskIoRouting.TryResolve(dtype, out MaskWireFormat format))
			throw new FormatException(MaskIoRouting.UnsupportedDtypeMessage(dtype, "CSV"));

		return format switch
		{
			MaskWireFormat.Packed => DeserializeMaskPacked(gpu, fields),
			MaskWireFormat.Bool => DeserializeMaskBool(gpu, fields),
			_ => throw new InvalidOperationException($"Unsupported mask wire format '{format}'."),
		};
	}

	static Mask DeserializeMaskBool(GPU gpu, IReadOnlyDictionary<string, string> fields)
	{
		var document = new MaskBoolDocument
		{
			SchemaVersion = ReadIntField(fields, IoSchema.Field.SchemaVersion),
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Data = ParseBoolData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion);
		StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask));
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(bool));
		StructuredIoValidation.ValidateMaskBoolLayout(document.Columns, document.Data.Length);

		return new Mask(gpu, document.Data, document.Columns);
	}

	static Mask DeserializeMaskPacked(GPU gpu, IReadOnlyDictionary<string, string> fields)
	{
		var document = new MaskPackedDocument
		{
			SchemaVersion = ReadIntField(fields, IoSchema.Field.SchemaVersion),
			Type = ReadStringField(fields, IoSchema.Field.Type),
			Dtype = ReadStringField(fields, IoSchema.Field.Dtype),
			Columns = ReadIntField(fields, IoSchema.Field.Columns),
			Count = ReadIntField(fields, IoSchema.Field.Count),
			Data = ParseIntData(ReadStringField(fields, IoSchema.Field.Data)),
		};

		StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion);
		StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask));
		StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(int));
		StructuredIoValidation.ValidateMaskPackedLayout(document.Columns, document.Count, document.Data.Length);

		return new Mask(gpu, document.Data, document.Count, document.Columns);
	}

	static string[] BuildMetadataValues(Type documentType, Type elementType, int columns, string dataField, int? count = null)
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

	static string BuildDocument(string[] header, string[] values)
	{
		if (header.Length != values.Length)
			throw new InvalidOperationException("CSV header and value column counts must match.");

		return string.Join(',', header) + '\n' + string.Join(',', values);
	}

	static IReadOnlyDictionary<string, string> ParseDocument(string csv)
	{
		// Metadata columns are fixed names with no embedded commas; the last column is 'data'
		// and uses semicolon-separated values internally (no commas in field values).
		string[] lines = csv
			.Replace("\r\n", "\n")
			.Replace('\r', '\n')
			.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (lines.Length != 2)
			throw new FormatException($"CSV document must contain exactly one header row and one data row. Received {lines.Length} non-empty lines.");

		string[] header = lines[0].Split(',');
		string[] values = lines[1].Split(',');

		if (header.Length != values.Length)
			throw new FormatException($"CSV header column count {header.Length} does not match data column count {values.Length}.");

		if (!string.Equals(header[^1], IoSchema.Field.Data, StringComparison.OrdinalIgnoreCase))
			throw new FormatException($"CSV header must end with a '{IoSchema.Field.Data}' column.");

		var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < header.Length; i++)
		{
			string name = header[i].Trim();
			if (name.Length == 0)
				throw new FormatException($"CSV header column {i + 1} is empty.");

			if (!fields.TryAdd(name, values[i].Trim()))
				throw new FormatException($"CSV header contains duplicate column '{name}'.");
		}

		return fields;
	}

	static string JoinDataField(IEnumerable<string> values) => string.Join(DataSeparator, values);

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
