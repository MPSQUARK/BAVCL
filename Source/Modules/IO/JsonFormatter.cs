using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BAVCL.Core.Helpers;
using BAVCL.Geometric;
using BAVCL.Modules.IO.Enums;
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

	const int CurrentSchemaVersion = 1;

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
		return SerializeFloatArray(JsonTypeNames.Vector, vector.Columns, vector.ToArray());
	}

	string SerializeVector3(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		vector.SyncCPU();
		return SerializeFloatArray(JsonTypeNames.Vector3, vector.Columns, vector.ToArray());
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
					SchemaVersion = CurrentSchemaVersion,
					Type = JsonTypeNames.Mask,
					Dtype = JsonDtypes.Int32,
					Columns = mask.Columns,
					Count = mask.ElementCount,
					Data = mask.ToWordArray(),
				},
				Serializer),
			MaskSerializeFlags.Bool => JsonSerializer.Serialize(
				new MaskBoolDocument
				{
					SchemaVersion = CurrentSchemaVersion,
					Type = JsonTypeNames.Mask,
					Dtype = JsonDtypes.Bool,
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
		FloatArrayDocument document = DeserializeFloatArray(text, JsonTypeNames.Vector, JsonDtypes.Float32);
		return new Vector(gpu, document.Data, document.Columns, cache: document.Data.Length > 0);
	}

	Vector3 DeserializeVector3(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		FloatArrayDocument document = DeserializeFloatArray(text, JsonTypeNames.Vector3, JsonDtypes.Float32);
		return new Vector3(gpu, document.Data, cache: document.Data.Length > 0);
	}

	Mask DeserializeMask(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		using JsonDocument document = JsonDocument.Parse(text);
		JsonElement root = document.RootElement;

		ValidateOptionalMetadata(root, JsonTypeNames.Mask, expectedDtype: null);

		if (root.TryGetProperty("count", out _))
			return DeserializeMaskPacked(gpu, root);

		if (root.TryGetProperty("data", out JsonElement data) && data.ValueKind == JsonValueKind.Array)
		{
			if (data.GetArrayLength() == 0)
				throw new JsonException("Mask JSON 'data' must contain at least one element.");

			if (data[0].ValueKind is JsonValueKind.True or JsonValueKind.False)
				return DeserializeMaskBool(gpu, root);

			if (data[0].ValueKind == JsonValueKind.Number)
				return DeserializeMaskPacked(gpu, root);
		}

		throw new JsonException("Mask JSON must use bool[] data or packed { count, data: int[] }.");
	}

	string SerializeFloatArray(string type, int columns, float[] data) =>
		JsonSerializer.Serialize(
			new FloatArrayDocument
			{
				SchemaVersion = CurrentSchemaVersion,
				Type = type,
				Dtype = JsonDtypes.Float32,
				Columns = columns,
				Data = data,
			},
			Serializer);

	static FloatArrayDocument DeserializeFloatArray(string json, string expectedType, string expectedDtype)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		FloatArrayDocument? document = JsonSerializer.Deserialize<FloatArrayDocument>(json, Serializer)
			?? throw new JsonException("JSON payload deserialized to null.");

		ValidateSchemaVersion(document.SchemaVersion);
		ValidateOptionalType(document.Type, expectedType);
		ValidateOptionalDtype(document.Dtype, expectedDtype);

		document.Data ??= [];
		if (document.Columns < 0)
			throw new JsonException($"JSON 'columns' must be >= 0. Received {document.Columns}.");

		return document;
	}

	static Mask DeserializeMaskPacked(GPU gpu, JsonElement root)
	{
		MaskPackedDocument? document = root.Deserialize<MaskPackedDocument>(Serializer)
			?? throw new JsonException("Mask packed JSON deserialized to null.");

		ValidateSchemaVersion(document.SchemaVersion);
		ValidateOptionalType(document.Type, JsonTypeNames.Mask);
		ValidateOptionalDtype(document.Dtype, JsonDtypes.Int32);

		document.Data ??= [];
		if (document.Columns < 0)
			throw new JsonException($"JSON 'columns' must be >= 0. Received {document.Columns}.");

		if (document.Count <= 0)
			throw new JsonException($"Mask packed JSON 'count' must be > 0. Received {document.Count}.");

		int expectedWords = MaskBitOps.WordCount(document.Count);
		if (document.Data.Length != expectedWords)
			throw new JsonException(
				$"Mask packed JSON word length {document.Data.Length} does not match count {document.Count} (expected {expectedWords} words).");

		return new Mask(gpu, document.Data, document.Count, document.Columns);
	}

	static Mask DeserializeMaskBool(GPU gpu, JsonElement root)
	{
		MaskBoolDocument? document = root.Deserialize<MaskBoolDocument>(Serializer)
			?? throw new JsonException("Mask bool JSON deserialized to null.");

		ValidateSchemaVersion(document.SchemaVersion);
		ValidateOptionalType(document.Type, JsonTypeNames.Mask);
		ValidateOptionalDtype(document.Dtype, JsonDtypes.Bool);

		document.Data ??= [];
		if (document.Columns < 0)
			throw new JsonException($"JSON 'columns' must be >= 0. Received {document.Columns}.");

		if (document.Data.Length == 0)
			throw new JsonException("Mask bool JSON 'data' must contain at least one element.");

		return new Mask(gpu, document.Data, document.Columns);
	}

	static void ValidateOptionalMetadata(JsonElement element, string expectedType, string? expectedDtype)
	{
		if (element.TryGetProperty("schemaVersion", out JsonElement versionElement)
			&& versionElement.TryGetInt32(out int version))
			ValidateSchemaVersion(version);

		if (element.TryGetProperty("type", out JsonElement typeElement)
			&& typeElement.ValueKind == JsonValueKind.String)
			ValidateOptionalType(typeElement.GetString(), expectedType);

		if (expectedDtype is not null
			&& element.TryGetProperty("dtype", out JsonElement dtypeElement)
			&& dtypeElement.ValueKind == JsonValueKind.String)
			ValidateOptionalDtype(dtypeElement.GetString(), expectedDtype);
	}

	static void ValidateSchemaVersion(int schemaVersion)
	{
		if (schemaVersion is not 0 and not CurrentSchemaVersion)
			throw new JsonException($"Unsupported schemaVersion {schemaVersion}. Supported: {CurrentSchemaVersion}.");
	}

	static void ValidateOptionalType(string? type, string expectedType)
	{
		if (type is null)
			return;

		if (!string.Equals(type, expectedType, StringComparison.Ordinal))
			throw new JsonException($"JSON type '{type}' does not match expected '{expectedType}'.");
	}

	static void ValidateOptionalDtype(string? dtype, string expectedDtype)
	{
		if (dtype is null)
			return;

		if (!string.Equals(dtype, expectedDtype, StringComparison.Ordinal))
			throw new JsonException($"JSON dtype '{dtype}' does not match expected '{expectedDtype}'.");
	}

	static class JsonTypeNames
	{
		internal const string Vector = "Vector";
		internal const string Vector3 = "Vector3";
		internal const string Mask = "Mask";
	}

	static class JsonDtypes
	{
		internal const string Float32 = "float32";
		internal const string Int32 = "int32";
		internal const string Bool = "bool";
	}

	sealed class FloatArrayDocument
	{
		public int SchemaVersion { get; set; } = 1;
		public string? Type { get; set; }
		public string? Dtype { get; set; }
		public int Columns { get; set; }
		public float[] Data { get; set; } = [];
	}

	sealed class MaskBoolDocument
	{
		public int SchemaVersion { get; set; } = 1;
		public string? Type { get; set; }
		public string? Dtype { get; set; }
		public int Columns { get; set; }
		public bool[] Data { get; set; } = [];
	}

	sealed class MaskPackedDocument
	{
		public int SchemaVersion { get; set; } = 1;
		public string? Type { get; set; }
		public string? Dtype { get; set; }
		public int Columns { get; set; }
		public int Count { get; set; }
		public int[] Data { get; set; } = [];
	}
}
