using System;
using System.Collections.Generic;
using System.Linq;
using BAVCL.Geometric;
using BAVCL.Modules.Structural;
using BAVCL.Core.Interfaces;
using BAVCL.Modules.IO.Internal;
using BAVCL.Types;

namespace BAVCL.Modules.IO;

/// <summary>Plain-text persistence via pipe-formatted ToStr output.</summary>
public sealed class TxtFormatter :
	IFormatter<Vector>,
	IFormatter<Vector3>,
	IFormatter<Mask>,
	ICollectionFormatter<Vector>,
	ICollectionFormatter<Vector3>,
	ICollectionFormatter<Mask>,
	ISingleton<TxtFormatter>
{
	public static TxtFormatter Default { get; } = new();

	static TxtFormatter ISingleton<TxtFormatter>.Default => Default;

	TxtFormatter() { }

	public string Extension => ".txt";

	string IFormatter<Vector>.Serialize(Vector vector, int flags) => SerializeVector(vector);

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text) => DeserializeVector(gpu, text);

	string IFormatter<Vector3>.Serialize(Vector3 vector, int flags) => SerializeVector3(vector);

	Vector3 IFormatter<Vector3>.Deserialize(GPU gpu, string text) => DeserializeVector3(gpu, text);

	string IFormatter<Mask>.Serialize(Mask mask, int flags) => SerializeMask(mask);

	Mask IFormatter<Mask>.Deserialize(GPU gpu, string text) => DeserializeMask(gpu, text);

	string ICollectionFormatter<Vector>.OpenCollection(Vector first, int flags) => SerializeVector(first);

	string ICollectionFormatter<Vector>.AppendItem(Vector value, int flags) => AppendSegment(SerializeVector(value));

	string ICollectionFormatter<Vector>.CloseCollection(int itemCount) => string.Empty;

	IReadOnlyList<Vector> ICollectionFormatter<Vector>.DeserializeAll(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return TxtBatchIo.SplitDocuments(text)
			.Select(segment =>
			{
				(float[] values, int columns) = TxtParsing.ParseFloatGrid(segment);
				return new Vector(gpu, values, columns, cache: values.Length > 0);
			})
			.ToList();
	}

	string ICollectionFormatter<Vector3>.OpenCollection(Vector3 first, int flags) => SerializeVector3(first);

	string ICollectionFormatter<Vector3>.AppendItem(Vector3 value, int flags) => AppendSegment(SerializeVector3(value));

	string ICollectionFormatter<Vector3>.CloseCollection(int itemCount) => string.Empty;

	IReadOnlyList<Vector3> ICollectionFormatter<Vector3>.DeserializeAll(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return TxtBatchIo.SplitDocuments(text)
			.Select(segment => ParseVector3(gpu, segment))
			.ToList();
	}

	string ICollectionFormatter<Mask>.OpenCollection(Mask first, int flags) => SerializeMask(first);

	string ICollectionFormatter<Mask>.AppendItem(Mask value, int flags) => AppendSegment(SerializeMask(value));

	string ICollectionFormatter<Mask>.CloseCollection(int itemCount) => string.Empty;

	IReadOnlyList<Mask> ICollectionFormatter<Mask>.DeserializeAll(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return TxtBatchIo.SplitDocuments(text)
			.Select(segment =>
			{
				(bool[] values, int columns) = TxtParsing.ParseBoolGrid(segment);
				return new Mask(gpu, values, columns);
			})
			.ToList();
	}

	static string SerializeVector(Vector vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return vector.ToString()!;
	}

	static string SerializeVector3(Vector3 vector)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return vector.ToString()!;
	}

	static string SerializeMask(Mask mask)
	{
		ArgumentNullException.ThrowIfNull(mask);
		return mask.ToStr();
	}

	static string AppendSegment(string fragment) => $"\n{IoSchema.Collection.TxtBoundary}\n{fragment}";

	static Vector DeserializeVector(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		(float[] values, int columns) = TxtParsing.ParseFloatGrid(text);
		return new Vector(gpu, values, columns, cache: values.Length > 0);
	}

	static Vector3 DeserializeVector3(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return ParseVector3(gpu, text);
	}

	static Vector3 ParseVector3(GPU gpu, string text)
	{
		(float[] values, int columns) = TxtParsing.ParseFloatGrid(text, requiredColumns: 3);
		StructuredIoValidation.ValidateVector3Layout(columns, values.Length);

		return new Vector3(gpu, values, cache: values.Length > 0);
	}

	static Mask DeserializeMask(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		(bool[] values, int columns) = TxtParsing.ParseBoolGrid(text);
		return new Mask(gpu, values, columns);
	}
}
