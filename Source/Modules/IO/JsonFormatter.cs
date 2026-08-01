using System;
using System.Collections.Generic;
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
	ICollectionFormatter<Vector>,
	ICollectionFormatter<Vector3>,
	ICollectionFormatter<Mask>,
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

	string ICollectionFormatter<Vector>.OpenCollection(Vector first, int flags) =>
		IoSchema.Collection.JsonOpen(StructuredIoValidation.CurrentSchemaVersion) + SerializeVectorItem(first);

	string ICollectionFormatter<Vector>.AppendItem(Vector value, int flags) => ',' + SerializeVectorItem(value);

	string ICollectionFormatter<Vector>.CloseCollection(int itemCount) => IoSchema.Collection.JsonClose;

	IReadOnlyList<Vector> ICollectionFormatter<Vector>.DeserializeAll(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		var results = new List<Vector>();
		foreach (FloatArrayDocument document in DeserializeAllFloatArray(text, typeof(Vector), typeof(float)))
			results.Add(new Vector(gpu, document.Data, document.Columns, cache: document.Data.Length > 0));

		return results;
	}

	string ICollectionFormatter<Vector3>.OpenCollection(Vector3 first, int flags) =>
		IoSchema.Collection.JsonOpen(StructuredIoValidation.CurrentSchemaVersion) + SerializeVector3Item(first);

	string ICollectionFormatter<Vector3>.AppendItem(Vector3 value, int flags) => ',' + SerializeVector3Item(value);

	string ICollectionFormatter<Vector3>.CloseCollection(int itemCount) => IoSchema.Collection.JsonClose;

	IReadOnlyList<Vector3> ICollectionFormatter<Vector3>.DeserializeAll(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		var results = new List<Vector3>();
		foreach (FloatArrayDocument document in DeserializeAllFloatArray(text, typeof(Vector3), typeof(float)))
		{
			ValidateJson(() => StructuredIoValidation.ValidateVector3Layout(document.Columns, document.Data.Length));
			results.Add(new Vector3(gpu, document.Data, cache: document.Data.Length > 0));
		}

		return results;
	}

	string ICollectionFormatter<Mask>.OpenCollection(Mask first, int flags) =>
		IoSchema.Collection.JsonOpen(StructuredIoValidation.CurrentSchemaVersion) + SerializeMaskItem(first, flags);

	string ICollectionFormatter<Mask>.AppendItem(Mask value, int flags) => ',' + SerializeMaskItem(value, flags);

	string ICollectionFormatter<Mask>.CloseCollection(int itemCount) => IoSchema.Collection.JsonClose;

	IReadOnlyList<Mask> ICollectionFormatter<Mask>.DeserializeAll(GPU gpu, string text) => DeserializeAllMask(gpu, text);

	string SerializeVector(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		// STJ serializes float[] only; span must be materialized (CSV/XML use RetrieveReadOnlySpan).
		return SerializeFloatArrayFragment(typeof(Vector), vector.Columns, vector.ToArray());
	}

	string SerializeVector3(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return SerializeFloatArrayFragment(typeof(Vector3), vector.Columns, vector.ToArray());
	}

	string SerializeVectorItem(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return SerializeFloatArrayItem(typeof(Vector), vector.Columns, vector.ToArray());
	}

	string SerializeVector3Item(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return SerializeFloatArrayItem(typeof(Vector3), vector.Columns, vector.ToArray());
	}

	string SerializeMaskItem(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);

		return flags switch
		{
			MaskSerializeFlags.Packed => JsonSerializer.Serialize(
				new MaskPackedItemDocument
				{
					Type = IoSchema.Document.Of<Mask>(),
					Dtype = IoSchema.Dtype.Of<int>(),
					Columns = mask.Columns,
					Count = mask.ElementCount,
					Data = mask.ToWordArray(),
				},
				Serializer),
			MaskSerializeFlags.Bool => JsonSerializer.Serialize(
				new MaskBoolItemDocument
				{
					Type = IoSchema.Document.Of<Mask>(),
					Dtype = IoSchema.Dtype.Of<bool>(),
					Columns = mask.Columns,
					Data = mask.ToBoolArray(),
				},
				Serializer),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};
	}

	string SerializeMask(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);

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
		return DeserializeMaskElement(gpu, document.RootElement);
	}

	static IReadOnlyList<Mask> DeserializeAllMask(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		using JsonDocument document = JsonDocument.Parse(text);
		JsonElement root = document.RootElement;
		if (root.ValueKind != JsonValueKind.Object)
			throw new JsonException("Mask JSON collection must be a top-level object.");

		int schemaVersion = ReadCollectionSchemaVersion(root);
		if (!root.TryGetProperty(IoSchema.Collection.Items, out JsonElement items)
			|| items.ValueKind != JsonValueKind.Array)
			throw new JsonException($"Mask JSON collection must contain an '{IoSchema.Collection.Items}' array.");

		var results = new List<Mask>(items.GetArrayLength());
		foreach (JsonElement element in items.EnumerateArray())
			results.Add(DeserializeMaskItemElement(gpu, element));

		return results;
	}

	static Mask DeserializeMaskItemElement(GPU gpu, JsonElement root)
	{
		ValidateCollectionItemMetadata(root, typeof(Mask), expectedElementType: null);
		return DeserializeMaskByDtype(gpu, root, DeserializeMaskPackedCollectionItem, DeserializeMaskBoolCollectionItem);
	}

	static Mask DeserializeMaskElement(GPU gpu, JsonElement root)
	{
		ValidateFragmentMetadata(root, typeof(Mask), expectedElementType: null);
		return DeserializeMaskByDtype(gpu, root, DeserializeMaskPackedFragment, DeserializeMaskBoolFragment);
	}

	static Mask DeserializeMaskByDtype(
		GPU gpu,
		JsonElement root,
		Func<GPU, JsonElement, Mask> deserializePacked,
		Func<GPU, JsonElement, Mask> deserializeBool)
	{
		if (!root.TryGetProperty(IoSchema.Field.Dtype, out JsonElement dtypeElement)
			|| dtypeElement.ValueKind != JsonValueKind.String)
			throw new JsonException($"Mask JSON '{IoSchema.Field.Dtype}' is required.");

		string? dtype = dtypeElement.GetString();

		if (!MaskIoRouting.TryResolve(dtype, out MaskWireFormat format))
			throw new JsonException(MaskIoRouting.UnsupportedDtypeMessage(dtype, "JSON"));

		return format switch
		{
			MaskWireFormat.Packed => deserializePacked(gpu, root),
			MaskWireFormat.Bool => deserializeBool(gpu, root),
			_ => throw new InvalidOperationException($"Unsupported mask wire format '{format}'."),
		};
	}

	string SerializeFloatArrayFragment(Type type, int columns, float[] data) =>
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

	string SerializeFloatArrayItem(Type type, int columns, float[] data) =>
		JsonSerializer.Serialize(
			new FloatArrayItemDocument
			{
				Type = IoSchema.Document.Of(type),
				Dtype = IoSchema.Dtype.Of<float>(),
				Columns = columns,
				Data = data,
			},
			Serializer);

	static FloatArrayDocument DeserializeFloatArray(string json, Type expectedType, Type expectedElementType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		using JsonDocument document = JsonDocument.Parse(json);
		return DeserializeFloatArrayElement(document.RootElement, expectedType, expectedElementType);
	}

	static IReadOnlyList<FloatArrayDocument> DeserializeAllFloatArray(string json, Type expectedType, Type expectedElementType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		using JsonDocument document = JsonDocument.Parse(json);
		JsonElement root = document.RootElement;
		if (root.ValueKind != JsonValueKind.Object)
			throw new JsonException("JSON collection must be a top-level object.");

		int schemaVersion = ReadCollectionSchemaVersion(root);
		if (!root.TryGetProperty(IoSchema.Collection.Items, out JsonElement items)
			|| items.ValueKind != JsonValueKind.Array)
			throw new JsonException($"JSON collection must contain an '{IoSchema.Collection.Items}' array.");

		var results = new List<FloatArrayDocument>(items.GetArrayLength());
		foreach (JsonElement element in items.EnumerateArray())
			results.Add(DeserializeFloatArrayItemElement(element, expectedType, expectedElementType, schemaVersion));

		return results;
	}

	static int ReadCollectionSchemaVersion(JsonElement root)
	{
		if (!root.TryGetProperty(IoSchema.Field.SchemaVersion, out JsonElement versionElement)
			|| !versionElement.TryGetInt32(out int schemaVersion))
			throw new JsonException($"JSON collection '{IoSchema.Field.SchemaVersion}' is required.");

		ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(schemaVersion));
		return schemaVersion;
	}

	static FloatArrayDocument DeserializeFloatArrayItemElement(
		JsonElement element,
		Type expectedType,
		Type expectedElementType,
		int schemaVersion)
	{
		FloatArrayItemDocument? document = element.Deserialize<FloatArrayItemDocument>(Serializer)
			?? throw new JsonException("JSON payload deserialized to null.");

		ValidateJson(() => StructuredIoValidation.ValidateOptionalType(document.Type, expectedType));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(document.Dtype, expectedElementType));

		document.Data ??= [];
		ValidateJson(() => StructuredIoValidation.ValidateColumns(document.Columns));

		return new FloatArrayDocument
		{
			SchemaVersion = schemaVersion,
			Type = document.Type,
			Dtype = document.Dtype,
			Columns = document.Columns,
			Data = document.Data,
		};
	}

	static FloatArrayDocument DeserializeFloatArrayElement(JsonElement element, Type expectedType, Type expectedElementType)
	{
		FloatArrayDocument? document = element.Deserialize<FloatArrayDocument>(Serializer)
			?? throw new JsonException("JSON payload deserialized to null.");

		ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(document.SchemaVersion));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalType(document.Type, expectedType));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(document.Dtype, expectedElementType));

		document.Data ??= [];
		ValidateJson(() => StructuredIoValidation.ValidateColumns(document.Columns));

		return document;
	}

	static Mask DeserializeMaskPackedCollectionItem(GPU gpu, JsonElement root) =>
		DeserializeMaskPackedCore(gpu, root, validateSchema: false);

	static Mask DeserializeMaskPackedFragment(GPU gpu, JsonElement root) =>
		DeserializeMaskPackedCore(gpu, root, validateSchema: true);

	static Mask DeserializeMaskPackedCore(GPU gpu, JsonElement root, bool validateSchema)
	{
		MaskPackedItemDocument? document = root.Deserialize<MaskPackedItemDocument>(Serializer)
			?? throw new JsonException("Mask packed JSON deserialized to null.");

		if (validateSchema)
		{
			if (!root.TryGetProperty(IoSchema.Field.SchemaVersion, out JsonElement versionElement)
				|| !versionElement.TryGetInt32(out int schemaVersion))
				throw new JsonException($"Mask packed JSON '{IoSchema.Field.SchemaVersion}' is required.");

			ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(schemaVersion));
		}

		ValidateJson(() => StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask)));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(int)));

		document.Data ??= [];
		ValidateJson(() => StructuredIoValidation.ValidateMaskPackedLayout(
			document.Columns,
			document.Count,
			document.Data.Length));

		return new Mask(gpu, document.Data, document.Count, document.Columns);
	}

	static Mask DeserializeMaskBoolCollectionItem(GPU gpu, JsonElement root) =>
		DeserializeMaskBoolCore(gpu, root, validateSchema: false);

	static Mask DeserializeMaskBoolFragment(GPU gpu, JsonElement root) =>
		DeserializeMaskBoolCore(gpu, root, validateSchema: true);

	static Mask DeserializeMaskBoolCore(GPU gpu, JsonElement root, bool validateSchema)
	{
		MaskBoolItemDocument? document = root.Deserialize<MaskBoolItemDocument>(Serializer)
			?? throw new JsonException("Mask bool JSON deserialized to null.");

		if (validateSchema)
		{
			if (!root.TryGetProperty(IoSchema.Field.SchemaVersion, out JsonElement versionElement)
				|| !versionElement.TryGetInt32(out int schemaVersion))
				throw new JsonException($"Mask bool JSON '{IoSchema.Field.SchemaVersion}' is required.");

			ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(schemaVersion));
		}

		ValidateJson(() => StructuredIoValidation.ValidateOptionalType(document.Type, typeof(Mask)));
		ValidateJson(() => StructuredIoValidation.ValidateOptionalDtype(document.Dtype, typeof(bool)));

		document.Data ??= [];
		ValidateJson(() => StructuredIoValidation.ValidateMaskBoolLayout(document.Columns, document.Data.Length));

		return new Mask(gpu, document.Data, document.Columns);
	}

	static void ValidateFragmentMetadata(JsonElement element, Type expectedType, Type? expectedElementType)
	{
		if (element.TryGetProperty(IoSchema.Field.SchemaVersion, out JsonElement versionElement)
			&& versionElement.TryGetInt32(out int version))
			ValidateJson(() => StructuredIoValidation.ValidateSchemaVersion(version));

		ValidateOptionalTypeAndDtype(element, expectedType, expectedElementType);
	}

	static void ValidateCollectionItemMetadata(JsonElement element, Type expectedType, Type? expectedElementType) =>
		ValidateOptionalTypeAndDtype(element, expectedType, expectedElementType);

	static void ValidateOptionalTypeAndDtype(JsonElement element, Type expectedType, Type? expectedElementType)
	{
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
