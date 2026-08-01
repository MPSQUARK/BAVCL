using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using BAVCL.Geometric;
using BAVCL.Modules.IO.Enums;
using BAVCL.Modules.IO.Internal;
using BAVCL.Modules.IO.Internal.Schema;
using BAVCL.Core.Interfaces;
using BAVCL.Types;

namespace BAVCL.Modules.IO;

/// <summary>XML persistence for Vector, Vector3, and Mask.</summary>
public sealed class XmlFormatter :
	IFormatter<Vector>,
	IFormatter<Vector3>,
	IFormatter<Mask>,
	ICollectionFormatter<Vector>,
	ICollectionFormatter<Vector3>,
	ICollectionFormatter<Mask>,
	ISingleton<XmlFormatter>
{
	public static XmlFormatter Default { get; } = new();

	static XmlFormatter ISingleton<XmlFormatter>.Default => Default;

	XmlFormatter() { }

	public string Extension => ".xml";

	string IFormatter<Vector>.Serialize(Vector value, int flags) => SerializeVector(value);

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text) => DeserializeVector(gpu, text);

	string IFormatter<Vector3>.Serialize(Vector3 value, int flags) => SerializeVector3(value);

	Vector3 IFormatter<Vector3>.Deserialize(GPU gpu, string text) => DeserializeVector3(gpu, text);

	string IFormatter<Mask>.Serialize(Mask mask, int flags) => SerializeMask(mask, flags);

	Mask IFormatter<Mask>.Deserialize(GPU gpu, string text) => DeserializeMask(gpu, text);

	string ICollectionFormatter<Vector>.OpenCollection(Vector first, int flags) =>
		IoSchema.Collection.XmlOpen(StructuredIoValidation.CurrentSchemaVersion) + SerializeVectorItem(first);

	string ICollectionFormatter<Vector>.AppendItem(Vector value, int flags) => SerializeVectorItem(value);

	string ICollectionFormatter<Vector>.CloseCollection(int itemCount) => IoSchema.Collection.XmlClose;

	IReadOnlyList<Vector> ICollectionFormatter<Vector>.DeserializeAll(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return DeserializeAllFloatArray(text, typeof(Vector), typeof(float))
			.Select(document => new Vector(gpu, document.Data, document.Columns, cache: document.Data.Length > 0))
			.ToList();
	}

	string ICollectionFormatter<Vector3>.OpenCollection(Vector3 first, int flags) =>
		IoSchema.Collection.XmlOpen(StructuredIoValidation.CurrentSchemaVersion) + SerializeVector3Item(first);

	string ICollectionFormatter<Vector3>.AppendItem(Vector3 value, int flags) => SerializeVector3Item(value);

	string ICollectionFormatter<Vector3>.CloseCollection(int itemCount) => IoSchema.Collection.XmlClose;

	IReadOnlyList<Vector3> ICollectionFormatter<Vector3>.DeserializeAll(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return DeserializeAllFloatArray(text, typeof(Vector3), typeof(float))
			.Select(document =>
			{
				StructuredIoValidation.ValidateVector3Layout(document.Columns, document.Data.Length);
				return new Vector3(gpu, document.Data, cache: document.Data.Length > 0);
			})
			.ToList();
	}

	string ICollectionFormatter<Mask>.OpenCollection(Mask first, int flags) =>
		IoSchema.Collection.XmlOpen(StructuredIoValidation.CurrentSchemaVersion) + SerializeMaskItem(first, flags);

	string ICollectionFormatter<Mask>.AppendItem(Mask value, int flags) => SerializeMaskItem(value, flags);

	string ICollectionFormatter<Mask>.CloseCollection(int itemCount) => IoSchema.Collection.XmlClose;

	IReadOnlyList<Mask> ICollectionFormatter<Mask>.DeserializeAll(GPU gpu, string text) => DeserializeAllMask(gpu, text);

	string SerializeVector(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return SerializeFloatArrayFragment(typeof(Vector), vector.Columns, vector.RetrieveReadOnlySpan());
	}

	string SerializeVector3(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return SerializeFloatArrayFragment(typeof(Vector3), vector.Columns, vector.RetrieveReadOnlySpan());
	}

	string SerializeVectorItem(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return SerializeFloatArrayItem(typeof(Vector), vector.Columns, vector.RetrieveReadOnlySpan());
	}

	string SerializeVector3Item(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return SerializeFloatArrayItem(typeof(Vector3), vector.Columns, vector.RetrieveReadOnlySpan());
	}

	string SerializeMaskItem(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);

		return flags switch
		{
			MaskSerializeFlags.Packed => SerializeMaskPackedItem(mask),
			MaskSerializeFlags.Bool => SerializeMaskBoolItem(mask),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};
	}

	string SerializeMask(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);

		return flags switch
		{
			MaskSerializeFlags.Packed => SerializeMaskPacked(mask),
			MaskSerializeFlags.Bool => SerializeMaskBool(mask),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};
	}

	static string SerializeFloatArrayFragment(Type type, int columns, ReadOnlySpan<float> data)
	{
		XElement root = CreateFloatArrayFragmentRoot(type, columns);
		AppendFloatDataElements(root, data);
		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static string SerializeFloatArrayItem(Type type, int columns, ReadOnlySpan<float> data)
	{
		XElement root = CreateFloatArrayItemRoot(type, columns);
		AppendFloatDataElements(root, data);
		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static XElement CreateFloatArrayFragmentRoot(Type type, int columns)
	{
		var root = new XElement(IoSchema.Document.XmlRootOf(type));
		root.Add(new XAttribute(IoSchema.Field.SchemaVersion, StructuredIoValidation.CurrentSchemaVersion));
		root.Add(
			new XAttribute(IoSchema.Field.Dtype, IoSchema.Dtype.Of<float>()),
			new XAttribute(IoSchema.Field.Columns, columns));
		return root;
	}

	static XElement CreateFloatArrayItemRoot(Type type, int columns)
	{
		var root = new XElement(IoSchema.Document.XmlRootOf(type));
		root.Add(
			new XAttribute(IoSchema.Field.Dtype, IoSchema.Dtype.Of<float>()),
			new XAttribute(IoSchema.Field.Columns, columns));
		return root;
	}

	static void AppendFloatDataElements(XElement root, ReadOnlySpan<float> data)
	{
		foreach (float value in data)
			root.Add(new XElement(IoSchema.Field.Data, FloatIoParsing.FormatFloat(value)));
	}

	static string SerializeMaskPacked(Mask mask)
	{
		XElement root = CreateMaskFragmentRoot(typeof(int), mask.Columns, mask.ElementCount);
		AppendMaskPackedDataElements(root, mask.RetrieveReadOnlySpan());
		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static string SerializeMaskPackedItem(Mask mask)
	{
		XElement root = CreateMaskItemRoot(typeof(int), mask.Columns, mask.ElementCount);
		AppendMaskPackedDataElements(root, mask.RetrieveReadOnlySpan());
		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static void AppendMaskPackedDataElements(XElement root, ReadOnlySpan<int> words)
	{
		foreach (int word in words)
			root.Add(new XElement(IoSchema.Field.Data, word.ToString(CultureInfo.InvariantCulture)));
	}

	static string SerializeMaskBool(Mask mask)
	{
		XElement root = CreateMaskFragmentRoot(typeof(bool), mask.Columns, count: null);
		AppendMaskBoolDataElements(root, mask);
		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static string SerializeMaskBoolItem(Mask mask)
	{
		XElement root = CreateMaskItemRoot(typeof(bool), mask.Columns, count: null);
		AppendMaskBoolDataElements(root, mask);
		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static void AppendMaskBoolDataElements(XElement root, Mask mask)
	{
		// Bool masks are stored packed (int32 words); unpacking to one bool per element always allocates.
		foreach (bool value in mask.ToBoolArray())
			root.Add(new XElement(IoSchema.Field.Data, BoolIoParsing.FormatXml(value)));
	}

	static XElement CreateMaskFragmentRoot(Type elementType, int columns, int? count)
	{
		var root = new XElement(IoSchema.Document.XmlRootOf<Mask>());
		root.Add(new XAttribute(IoSchema.Field.SchemaVersion, StructuredIoValidation.CurrentSchemaVersion));
		AppendMaskAttributes(root, elementType, columns, count);
		return root;
	}

	static XElement CreateMaskItemRoot(Type elementType, int columns, int? count)
	{
		var root = new XElement(IoSchema.Document.XmlRootOf<Mask>());
		AppendMaskAttributes(root, elementType, columns, count);
		return root;
	}

	static void AppendMaskAttributes(XElement root, Type elementType, int columns, int? count)
	{
		root.Add(
			new XAttribute(IoSchema.Field.Dtype, IoSchema.Dtype.Of(elementType)),
			new XAttribute(IoSchema.Field.Columns, columns));

		if (count is int packedCount)
			root.Add(new XAttribute(IoSchema.Field.Count, packedCount));
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
		StructuredIoValidation.ValidateVector3Layout(document.Columns, document.Data.Length);
		return new Vector3(gpu, document.Data, cache: document.Data.Length > 0);
	}

	Mask DeserializeMask(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		XElement root = XDocument.Parse(text).Root
			?? throw new FormatException("XML document must have a root element.");

		return DeserializeMaskElement(gpu, root);
	}

	static IReadOnlyList<Mask> DeserializeAllMask(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		XElement root = ValidateCollectionRoot(text);
		int schemaVersion = ReadIntAttribute(root, IoSchema.Field.SchemaVersion);
		StructuredIoValidation.ValidateSchemaVersion(schemaVersion);

		return root.Elements().Select(element => DeserializeMaskItemElement(gpu, element)).ToList();
	}

	static Mask DeserializeMaskItemElement(GPU gpu, XElement root) =>
		DeserializeMaskByDtype(gpu, root, DeserializeMaskPackedCollectionItem, DeserializeMaskBoolCollectionItem);

	static Mask DeserializeMaskElement(GPU gpu, XElement root) =>
		DeserializeMaskByDtype(gpu, root, DeserializeMaskPackedFragment, DeserializeMaskBoolFragment);

	static Mask DeserializeMaskByDtype(
		GPU gpu,
		XElement root,
		Func<GPU, XElement, Mask> deserializePacked,
		Func<GPU, XElement, Mask> deserializeBool)
	{
		ValidateRootName(root, typeof(Mask));

		string? dtype = root.Attribute(IoSchema.Field.Dtype)?.Value;
		if (dtype is null)
			throw new FormatException($"Mask XML '{IoSchema.Field.Dtype}' attribute is required.");

		if (!MaskIoRouting.TryResolve(dtype, out MaskWireFormat format))
			throw new FormatException(MaskIoRouting.UnsupportedDtypeMessage(dtype, "XML"));

		return format switch
		{
			MaskWireFormat.Packed => deserializePacked(gpu, root),
			MaskWireFormat.Bool => deserializeBool(gpu, root),
			_ => throw new InvalidOperationException($"Unsupported mask wire format '{format}'."),
		};
	}

	static FloatArrayDocument DeserializeFloatArray(string xml, Type expectedType, Type expectedElementType)
	{
		XElement root = XDocument.Parse(xml).Root
			?? throw new FormatException("XML document must have a root element.");

		return DeserializeFloatArrayElement(root, expectedType, expectedElementType);
	}

	static IReadOnlyList<FloatArrayDocument> DeserializeAllFloatArray(string xml, Type expectedType, Type expectedElementType)
	{
		XElement root = ValidateCollectionRoot(xml);
		int schemaVersion = ReadIntAttribute(root, IoSchema.Field.SchemaVersion);
		StructuredIoValidation.ValidateSchemaVersion(schemaVersion);

		return root.Elements()
			.Select(element => DeserializeFloatArrayItemElement(element, expectedType, expectedElementType, schemaVersion))
			.ToList();
	}

	static XElement ValidateCollectionRoot(string xml)
	{
		XElement root = XDocument.Parse(xml).Root
			?? throw new FormatException("XML document must have a root element.");

		if (!string.Equals(root.Name.LocalName, IoSchema.Collection.XmlRoot, StringComparison.OrdinalIgnoreCase))
			throw new FormatException($"XML collection root '{root.Name.LocalName}' does not match expected '{IoSchema.Collection.XmlRoot}'.");

		return root;
	}

	static FloatArrayDocument DeserializeFloatArrayItemElement(
		XElement element,
		Type expectedType,
		Type expectedElementType,
		int schemaVersion)
	{
		ValidateRootName(element, expectedType);
		ValidateCollectionItemElementMetadata(element, expectedElementType);

		int columns = ReadIntAttribute(element, IoSchema.Field.Columns);
		StructuredIoValidation.ValidateColumns(columns);

		float[] data = FloatArrayIo.ParseDataElements(element.Elements(IoSchema.Field.Data).Select(e => e.Value));

		return new FloatArrayDocument
		{
			SchemaVersion = schemaVersion,
			Type = IoSchema.Document.Of(expectedType),
			Dtype = ReadOptionalStringAttribute(element, IoSchema.Field.Dtype),
			Columns = columns,
			Data = data,
		};
	}

	static FloatArrayDocument DeserializeFloatArrayElement(XElement element, Type expectedType, Type expectedElementType)
	{
		ValidateRootName(element, expectedType);
		ValidateFragmentElementMetadata(element, expectedElementType);

		int columns = ReadIntAttribute(element, IoSchema.Field.Columns);
		StructuredIoValidation.ValidateColumns(columns);

		float[] data = FloatArrayIo.ParseDataElements(element.Elements(IoSchema.Field.Data).Select(e => e.Value));

		return new FloatArrayDocument
		{
			SchemaVersion = ReadSchemaVersion(element),
			Type = IoSchema.Document.Of(expectedType),
			Dtype = ReadOptionalStringAttribute(element, IoSchema.Field.Dtype),
			Columns = columns,
			Data = data,
		};
	}

	static Mask DeserializeMaskPackedFragment(GPU gpu, XElement root)
	{
		ValidateFragmentElementMetadata(root);
		ValidateOptionalDtype(root, typeof(int));
		return BuildMaskPacked(gpu, root);
	}

	static Mask DeserializeMaskPackedCollectionItem(GPU gpu, XElement root)
	{
		ValidateOptionalDtype(root, typeof(int));
		return BuildMaskPacked(gpu, root);
	}

	static Mask BuildMaskPacked(GPU gpu, XElement root)
	{
		int columns = ReadIntAttribute(root, IoSchema.Field.Columns);
		int count = ReadIntAttribute(root, IoSchema.Field.Count);
		int[] words = root.Elements(IoSchema.Field.Data)
			.Select(element => int.Parse(element.Value, NumberStyles.Integer, CultureInfo.InvariantCulture))
			.ToArray();

		StructuredIoValidation.ValidateMaskPackedLayout(columns, count, words.Length);

		return new Mask(gpu, words, count, columns);
	}

	static Mask DeserializeMaskBoolFragment(GPU gpu, XElement root)
	{
		ValidateFragmentElementMetadata(root);
		ValidateOptionalDtype(root, typeof(bool));
		return BuildMaskBool(gpu, root);
	}

	static Mask DeserializeMaskBoolCollectionItem(GPU gpu, XElement root)
	{
		ValidateOptionalDtype(root, typeof(bool));
		return BuildMaskBool(gpu, root);
	}

	static Mask BuildMaskBool(GPU gpu, XElement root)
	{
		int columns = ReadIntAttribute(root, IoSchema.Field.Columns);
		bool[] data = root.Elements(IoSchema.Field.Data)
			.Select(element => BoolIoParsing.ParseWireToken(element.Value))
			.ToArray();

		StructuredIoValidation.ValidateMaskBoolLayout(columns, data.Length);

		return new Mask(gpu, data, columns);
	}

	static void ValidateRootName(XElement root, Type expectedType)
	{
		string expectedRoot = IoSchema.Document.XmlRootOf(expectedType);
		if (!string.Equals(root.Name.LocalName, expectedRoot, StringComparison.OrdinalIgnoreCase))
			throw new FormatException($"XML root '{root.Name.LocalName}' does not match expected '{expectedRoot}'.");
	}

	static void ValidateFragmentElementMetadata(XElement element, Type expectedElementType)
	{
		ValidateOptionalSchemaVersion(element);
		ValidateOptionalDtype(element, expectedElementType);
	}

	static void ValidateCollectionItemElementMetadata(XElement element, Type expectedElementType) =>
		ValidateOptionalDtype(element, expectedElementType);

	static void ValidateFragmentElementMetadata(XElement element) =>
		ValidateOptionalSchemaVersion(element);

	static void ValidateOptionalSchemaVersion(XElement element)
	{
		if (element.Attribute(IoSchema.Field.SchemaVersion) is not XAttribute versionAttribute)
			return;

		if (int.TryParse(versionAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int version))
			StructuredIoValidation.ValidateSchemaVersion(version);
	}
	static void ValidateOptionalDtype(XElement root, Type expectedElementType)
	{
		if (root.Attribute(IoSchema.Field.Dtype) is XAttribute dtypeAttribute)
			StructuredIoValidation.ValidateOptionalDtype(dtypeAttribute.Value, expectedElementType);
	}

	/// <summary>
	/// Reads fragment <c>schemaVersion</c>; defaults to <see cref="StructuredIoValidation.CurrentSchemaVersion"/>
	/// when the attribute is absent (hand-crafted fragments without version metadata still deserialize).
	/// </summary>
	static int ReadSchemaVersion(XElement root) =>
		root.Attribute(IoSchema.Field.SchemaVersion) is XAttribute attribute
			? int.Parse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture)
			: StructuredIoValidation.CurrentSchemaVersion;

	static int ReadIntAttribute(XElement root, string name) =>
		root.Attribute(name) is XAttribute attribute
			? int.Parse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture)
			: throw new FormatException($"XML attribute '{name}' is required.");

	static string? ReadOptionalStringAttribute(XElement root, string name) =>
		root.Attribute(name)?.Value;
}
