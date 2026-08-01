using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BAVCL.Geometric;
using BAVCL.Modules.IO.Enums;
using BAVCL.Modules.IO.Internal;
using BAVCL.Modules.IO.Internal.Schema;
using BAVCL.Core.Interfaces;
using BAVCL.Types;

namespace BAVCL.Modules.IO;

/// <summary>JSON persistence for Vector, Vector3, and Mask.</summary>
public sealed class JsonFormatter :
	IFormatter<Vector>,
	IFormatter<Vector3>,
	IFormatter<Mask>,
	ISingleton<JsonFormatter>
{
	public static JsonFormatter Default { get; } = new();

	static JsonFormatter ISingleton<JsonFormatter>.Default => Default;

	static readonly JsonSerializerOptions Serializer = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
	};

	JsonFormatter() { }

	public string Extension => ".json";

	string IFormatter<Vector>.Serialize(Vector value, int flags) => SerializeVector(value);

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text) => DeserializeVector(gpu, text);

	string IFormatter<Vector3>.Serialize(Vector3 value, int flags) => SerializeVector3(value);

	Vector3 IFormatter<Vector3>.Deserialize(GPU gpu, string text) => DeserializeVector3(gpu, text);

	string IFormatter<Mask>.Serialize(Mask mask, int flags) => SerializeMask(mask, flags);

	Mask IFormatter<Mask>.Deserialize(GPU gpu, string text) => DeserializeMask(gpu, text);

	string SerializeVector(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		vector.SyncCPU();
		return SerializeFloatArray(typeof(Vector), vector.Columns, vector.ToArray());
	}

	string SerializeVector3(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		vector.SyncCPU();
		return SerializeFloatArray(typeof(Vector3), vector.Columns, vector.ToArray());
	}

	string SerializeMask(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);
		mask.SyncCPU();

		return flags switch
		{
			MaskSerializeFlags.Packed => JsonSerializer.Serialize(
				new MaskPackedDocument
				{
					SchemaVersion = StructuredIoValidation.CurrentSchemaVersion,
					Type = IoSchema.Document.Of<Mask>(),
					Dtype = IoSchema.Dtype.Of<int>(),
					Columns = mask.Columns,
					Count = mask.ElementCount,
					Data = mask.ToWordArray(),
				},
				Serializer),
			MaskSerializeFlags.Bool => JsonSerializer.Serialize(
				new MaskBoolDocument
				{
					SchemaVersion = StructuredIoValidation.CurrentSchemaVersion,
					Type = IoSchema.Document.Of<Mask>(),
					Dtype = IoSchema.Dtype.Of<bool>(),
					Columns = mask.Columns,
					Data = mask.ToBoolArray(),
				},
				Serializer),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};
	}

	Vector DeserializeVector(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		FloatArrayDocument document = DeserializeFloatArray(text, typeof(Vector), typeof(float));
		return new Vector(gpu, document.Data, document.Columns, cache: document.Data.Length > 0);
	}

	Vector3 DeserializeVector3(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		FloatArrayDocument document = DeserializeFloatArray(text, typeof(Vector3), typeof(float));
		ValidateJson(() => StructuredIoValidation.ValidateVector3Layout(document.Columns, document.Data.Length));
		return new Vector3(gpu, document.Data, cache: document.Data.Length > 0);
	}

	Mask DeserializeMask(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		using JsonDocument document = JsonDocument.Parse(text);
		JsonElement root = document.RootElement;

		ValidateOptionalMetadata(root, typeof(Mask), expectedElementType: null);

		if (!root.TryGetProperty(IoSchema.Field.Dtype, out JsonElement dtypeElement)
			|| dtypeElement.ValueKind != JsonValueKind.String)
			throw new JsonException($"Mask JSON '{IoSchema.Field.Dtype}' is required.");

		string? dtype = dtypeElement.GetString();

		if (!MaskIoRouting.TryResolve(dtype, out MaskWireFormat format))
			throw new JsonException(MaskIoRouting.UnsupportedDtypeMessage(dtype, "JSON"));

		return format switch
		{
			MaskWireFormat.Packed => DeserializeMaskPacked(gpu, root),
			MaskWireFormat.Bool => DeserializeMaskBool(gpu, root),
			_ => throw new InvalidOperationException($"Unsupported mask wire format '{format}'."),
		};
	}

	string SerializeFloatArray(Type type, int columns, float[] data) =>
		JsonSerializer.Serialize(
			new FloatArrayDocument
			{
				SchemaVersion = StructuredIoValidation.CurrentSchemaVersion,
				Type = IoSchema.Document.Of(type),
				Dtype = IoSchema.Dtype.Of<float>(),
				Columns = columns,
				Data = data,
			},
			Serializer);

	static FloatArrayDocument DeserializeFloatArray(string json, Type expectedType, Type expectedElementType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		FloatArrayDocument? document = JsonSerializer.Deserialize<FloatArrayDocument>(json, Serializer)
			?? throw new JsonException("JSON payload deserialized to null.");

		ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalType(document.Type, expectedType));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(document.Dtype, expectedElementType));

		document.Data ??= [];
		ValidateJson(() => StructuredIoValidation.ValidateColumns(document.Columns));

		return document;
	}

	static Mask DeserializeMaskPacked(GPU gpu, JsonElement root)
	{
		MaskPackedDocument? document = root.Deserialize<MaskPackedDocument>(Serializer)
			?? throw new JsonException("Mask packed JSON deserialized to null.");

		ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask)));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(int)));

		document.Data ??= [];
		ValidateJson(() => StructuredIoValidation.ValidateMaskPackedLayout(
			document.Columns,
			document.Count,
			document.Data.Length));

		return new Mask(gpu, document.Data, document.Count, document.Columns);
	}

	static Mask DeserializeMaskBool(GPU gpu, JsonElement root)
	{
		MaskBoolDocument? document = root.Deserialize<MaskBoolDocument>(Serializer)
			?? throw new JsonException("Mask bool JSON deserialized to null.");

		ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask)));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(bool)));

		document.Data ??= [];
		ValidateJson(() => StructuredIoValidation.ValidateMaskBoolLayout(document.Columns, document.Data.Length));

		return new Mask(gpu, document.Data, document.Columns);
	}

	static void ValidateOptionalMetadata(JsonElement element, Type expectedType, Type? expectedElementType)
	{
		if (element.TryGetProperty(IoSchema.Field.SchemaVersion, out JsonElement versionElement)
			&& versionElement.TryGetInt32(out int version))
			ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(version));

		if (element.TryGetProperty(IoSchema.Field.Type, out JsonElement typeElement)
			&& typeElement.ValueKind == JsonValueKind.String)
			ValidateJson(() => StructuredIoValidation.ValidateOptionalType(typeElement.GetString(), expectedType));

		if (expectedElementType is not null
			&& element.TryGetProperty(IoSchema.Field.Dtype, out JsonElement dtypeElement)
			&& dtypeElement.ValueKind == JsonValueKind.String)
			ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(dtypeElement.GetString(), expectedElementType));
	}

	static void ValidateJson(Action validate)
	{
		try
		{
			validate();
		}
		catch (FormatException ex)
		{
			throw new JsonException(ex.Message, ex);
		}
	}
}
