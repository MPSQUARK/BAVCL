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
			MaskSerializeFlags.Packed => SerializeMaskPacked(mask),
			MaskSerializeFlags.Bool => SerializeMaskBool(mask),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};
	}

	static string SerializeFloatArray(Type type, int columns, float[] data)
	{
		var root = new XElement(
			IoSchema.Document.XmlRootOf(type),
			new XAttribute(IoSchema.Field.SchemaVersion, StructuredIoValidation.CurrentSchemaVersion),
			new XAttribute(IoSchema.Field.Type, IoSchema.Document.Of(type)),
			new XAttribute(IoSchema.Field.Dtype, IoSchema.Dtype.Of<float>()),
			new XAttribute(IoSchema.Field.Columns, columns));

		foreach (float value in data)
			root.Add(new XElement(IoSchema.Field.Data, FloatIoParsing.FormatFloat(value)));

		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static string SerializeMaskPacked(Mask mask)
	{
		XElement root = CreateMaskRoot(typeof(int), mask.Columns, mask.ElementCount);

		foreach (int word in mask.ToWordArray())
			root.Add(new XElement(IoSchema.Field.Data, word.ToString(CultureInfo.InvariantCulture)));

		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static string SerializeMaskBool(Mask mask)
	{
		XElement root = CreateMaskRoot(typeof(bool), mask.Columns);

		foreach (bool value in mask.ToBoolArray())
			root.Add(new XElement(IoSchema.Field.Data, BoolIoParsing.FormatXml(value)));

		return new XDocument(root).ToString(SaveOptions.DisableFormatting);
	}

	static XElement CreateMaskRoot(Type elementType, int columns, int? count = null)
	{
		var root = new XElement(
			IoSchema.Document.XmlRootOf<Mask>(),
			new XAttribute(IoSchema.Field.SchemaVersion, StructuredIoValidation.CurrentSchemaVersion),
			new XAttribute(IoSchema.Field.Type, IoSchema.Document.Of<Mask>()),
			new XAttribute(IoSchema.Field.Dtype, IoSchema.Dtype.Of(elementType)),
			new XAttribute(IoSchema.Field.Columns, columns));

		if (count is int packedCount)
			root.Add(new XAttribute(IoSchema.Field.Count, packedCount));

		return root;
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

		ValidateRootName(root, typeof(Mask));
		ValidateOptionalMetadata(root, typeof(Mask), expectedElementType: null);

		string? dtype = root.Attribute(IoSchema.Field.Dtype)?.Value;
		if (dtype is null)
			throw new FormatException($"Mask XML '{IoSchema.Field.Dtype}' attribute is required.");

		if (!MaskIoRouting.TryResolve(dtype, out MaskWireFormat format))
			throw new FormatException(MaskIoRouting.UnsupportedDtypeMessage(dtype, "XML"));

		return format switch
		{
			MaskWireFormat.Packed => DeserializeMaskPacked(gpu, root),
			MaskWireFormat.Bool => DeserializeMaskBool(gpu, root),
			_ => throw new InvalidOperationException($"Unsupported mask wire format '{format}'."),
		};
	}

	static FloatArrayDocument DeserializeFloatArray(string xml, Type expectedType, Type expectedElementType)
	{
		XElement root = XDocument.Parse(xml).Root
			?? throw new FormatException("XML document must have a root element.");

		ValidateRootName(root, expectedType);
		ValidateOptionalMetadata(root, expectedType, expectedElementType);

		int columns = ReadIntAttribute(root, IoSchema.Field.Columns);
		StructuredIoValidation.ValidateColumns(columns);

		float[] data = FloatArrayIo.ParseDataElements(root.Elements(IoSchema.Field.Data).Select(element => element.Value));

		return new FloatArrayDocument
		{
			SchemaVersion = ReadSchemaVersion(root),
			Type = ReadOptionalStringAttribute(root, IoSchema.Field.Type),
			Dtype = ReadOptionalStringAttribute(root, IoSchema.Field.Dtype),
			Columns = columns,
			Data = data,
		};
	}

	static Mask DeserializeMaskPacked(GPU gpu, XElement root)
	{
		ValidateOptionalMetadata(root, typeof(Mask), typeof(int));

		int columns = ReadIntAttribute(root, IoSchema.Field.Columns);
		int count = ReadIntAttribute(root, IoSchema.Field.Count);
		int[] words = root.Elements(IoSchema.Field.Data)
			.Select(element => int.Parse(element.Value, NumberStyles.Integer, CultureInfo.InvariantCulture))
			.ToArray();

		StructuredIoValidation.ValidateMaskPackedLayout(columns, count, words.Length);

		return new Mask(gpu, words, count, columns);
	}

	static Mask DeserializeMaskBool(GPU gpu, XElement root)
	{
		ValidateOptionalMetadata(root, typeof(Mask), typeof(bool));

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

	static void ValidateOptionalMetadata(XElement root, Type expectedType, Type? expectedElementType)
	{
		if (root.Attribute(IoSchema.Field.SchemaVersion) is XAttribute versionAttribute
			&& int.TryParse(versionAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int version))
			StructuredIoValidation.ValidateSchemaVersion(version);

		if (root.Attribute(IoSchema.Field.Type) is XAttribute typeAttribute)
			StructuredIoValidation.ValidateOptionalType(typeAttribute.Value, expectedType);

		if (expectedElementType is not null && root.Attribute(IoSchema.Field.Dtype) is XAttribute dtypeAttribute)
			StructuredIoValidation.ValidateOptionalDtype(dtypeAttribute.Value, expectedElementType);
	}

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
