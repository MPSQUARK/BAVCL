using System;
using System.Collections.Generic;
using System.Linq;
using BAVCL.Geometric;
using BAVCL.Core.Interfaces;
using BAVCL.Modules.IO.Enums;
using BAVCL.Modules.IO.Internal;
using BAVCL.Modules.IO.Internal.Schema;
using BAVCL.Types;

namespace BAVCL.Modules.IO;

/// <summary>JSON-aligned CSV persistence for Vector, Vector3, and Mask.</summary>
public sealed class CsvFormatter :
	IFormatter<Vector>,
	IFormatter<VectorInt>,
	IFormatter<Vector3>,
	IFormatter<Mask>,
	ICollectionFormatter<Vector>,
	ICollectionFormatter<VectorInt>,
	ICollectionFormatter<Vector3>,
	ICollectionFormatter<Mask>,
	ISingleton<CsvFormatter>
{
	public static CsvFormatter Default { get; } = new();

	static CsvFormatter ISingleton<CsvFormatter>.Default => Default;

	CsvFormatter() { }

	public string Extension => ".csv";

	string IFormatter<Vector>.Serialize(Vector vector, int flags) => SerializeVector(vector);

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text) => DeserializeVector(gpu, text);

	string IFormatter<VectorInt>.Serialize(VectorInt vector, int flags) => SerializeVectorInt(vector);

	VectorInt IFormatter<VectorInt>.Deserialize(GPU gpu, string text) => DeserializeVectorInt(gpu, text);

	string IFormatter<Vector3>.Serialize(Vector3 vector, int flags) => SerializeVector3(vector);

	Vector3 IFormatter<Vector3>.Deserialize(GPU gpu, string text) => DeserializeVector3(gpu, text);

	string IFormatter<Mask>.Serialize(Mask mask, int flags) => SerializeMask(mask, flags);

	Mask IFormatter<Mask>.Deserialize(GPU gpu, string text) => StructuredCsv.DeserializeMask(gpu, text);

	string ICollectionFormatter<Vector>.OpenCollection(Vector first, int flags) =>
		StructuredCsv.OpenFloatArrayCollection(typeof(Vector), first.Columns, first.RetrieveReadOnlySpan());

	string ICollectionFormatter<Vector>.AppendItem(Vector value, int flags) =>
		$"{Environment.NewLine}{StructuredCsv.SerializeFloatArrayItemRow(typeof(Vector), value.Columns, value.RetrieveReadOnlySpan())}";

	string ICollectionFormatter<Vector>.CloseCollection(int itemCount) => string.Empty;

	IReadOnlyList<Vector> ICollectionFormatter<Vector>.DeserializeAll(GPU gpu, string text) =>
		StructuredCsv.DeserializeAllFloatArray(text, typeof(Vector))
			.Select(document => new Vector(gpu, document.Data, document.Columns, cache: document.Data.Length > 0))
			.ToList();

	string ICollectionFormatter<VectorInt>.OpenCollection(VectorInt first, int flags) =>
		StructuredCsv.OpenIntArrayCollection(typeof(VectorInt), first.Columns, first.RetrieveReadOnlySpan());

	string ICollectionFormatter<VectorInt>.AppendItem(VectorInt value, int flags) =>
		$"{Environment.NewLine}{StructuredCsv.SerializeIntArrayItemRow(typeof(VectorInt), value.Columns, value.RetrieveReadOnlySpan())}";

	string ICollectionFormatter<VectorInt>.CloseCollection(int itemCount) => string.Empty;

	IReadOnlyList<VectorInt> ICollectionFormatter<VectorInt>.DeserializeAll(GPU gpu, string text) =>
		StructuredCsv.DeserializeAllIntArray(text, typeof(VectorInt))
			.Select(document => new VectorInt(gpu, document.Data, document.Columns, cache: document.Data.Length > 0))
			.ToList();

	string ICollectionFormatter<Vector3>.OpenCollection(Vector3 first, int flags) =>
		StructuredCsv.OpenFloatArrayCollection(typeof(Vector3), first.Columns, first.RetrieveReadOnlySpan());

	string ICollectionFormatter<Vector3>.AppendItem(Vector3 value, int flags) =>
		$"{Environment.NewLine}{StructuredCsv.SerializeFloatArrayItemRow(typeof(Vector3), value.Columns, value.RetrieveReadOnlySpan())}";

	string ICollectionFormatter<Vector3>.CloseCollection(int itemCount) => string.Empty;

	IReadOnlyList<Vector3> ICollectionFormatter<Vector3>.DeserializeAll(GPU gpu, string text) =>
		StructuredCsv.DeserializeAllFloatArray(text, typeof(Vector3))
			.Select(document =>
			{
				StructuredIoValidation.ValidateVector3Layout(document.Columns, document.Data.Length);
				return new Vector3(gpu, document.Data, cache: document.Data.Length > 0);
			})
			.ToList();

	string ICollectionFormatter<Mask>.OpenCollection(Mask first, int flags) =>
		flags switch
		{
			MaskSerializeFlags.Packed => StructuredCsv.OpenMaskPackedCollection(
				first.Columns,
				first.ElementCount,
				first.RetrieveReadOnlySpan()),
			MaskSerializeFlags.Bool => StructuredCsv.OpenMaskBoolCollection(first.Columns, first.ToBoolArray()),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};

	string ICollectionFormatter<Mask>.AppendItem(Mask value, int flags) =>
		$"{Environment.NewLine}{flags switch
		{
			MaskSerializeFlags.Packed => StructuredCsv.SerializeMaskPackedItemRow(value.Columns, value.ElementCount, value.RetrieveReadOnlySpan()),
			MaskSerializeFlags.Bool => StructuredCsv.SerializeMaskBoolItemRow(value.Columns, value.ToBoolArray()),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		}}";

	string ICollectionFormatter<Mask>.CloseCollection(int itemCount) => string.Empty;

	IReadOnlyList<Mask> ICollectionFormatter<Mask>.DeserializeAll(GPU gpu, string text) =>
		StructuredCsv.DeserializeAllMask(gpu, text);

	static string SerializeVector(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return StructuredCsv.SerializeFloatArray(typeof(Vector), vector.Columns, vector.RetrieveReadOnlySpan());
	}

	static string SerializeVectorInt(VectorInt vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return StructuredCsv.SerializeIntArray(typeof(VectorInt), vector.Columns, vector.RetrieveReadOnlySpan());
	}

	static string SerializeVector3(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return StructuredCsv.SerializeFloatArray(typeof(Vector3), vector.Columns, vector.RetrieveReadOnlySpan());
	}

	static string SerializeMask(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);

		return flags switch
		{
			MaskSerializeFlags.Packed => StructuredCsv.SerializeMaskPacked(
				mask.Columns,
				mask.ElementCount,
				mask.RetrieveReadOnlySpan()),
			// Bool masks are stored packed (int32 words); unpacking to one bool per element always allocates.
			MaskSerializeFlags.Bool => StructuredCsv.SerializeMaskBool(mask.Columns, mask.ToBoolArray()),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};
	}

	static Vector DeserializeVector(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		FloatArrayDocument document = StructuredCsv.DeserializeFloatArray(text, typeof(Vector));
		return new Vector(gpu, document.Data, document.Columns, cache: document.Data.Length > 0);
	}

	static VectorInt DeserializeVectorInt(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		IntArrayDocument document = StructuredCsv.DeserializeIntArray(text, typeof(VectorInt));
		return new VectorInt(gpu, document.Data, document.Columns, cache: document.Data.Length > 0);
	}

	static Vector3 DeserializeVector3(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		FloatArrayDocument document = StructuredCsv.DeserializeFloatArray(text, typeof(Vector3));
		StructuredIoValidation.ValidateVector3Layout(document.Columns, document.Data.Length);
		return new Vector3(gpu, document.Data, cache: document.Data.Length > 0);
	}
}
