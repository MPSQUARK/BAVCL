using System;
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
	IFormatter<Vector3>,
	IFormatter<Mask>,
	ISingleton<CsvFormatter>
{
	public static CsvFormatter Default { get; } = new();

	static CsvFormatter ISingleton<CsvFormatter>.Default => Default;

	public string Extension => ".csv";

	string IFormatter<Vector>.Serialize(Vector vector, int flags)
	{
		ArgumentNullException.ThrowIfNull(vector);
		vector.SyncCPU();
		return StructuredCsv.SerializeFloatArray(typeof(Vector), vector.Columns, vector.ToArray());
	}

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		FloatArrayDocument document = StructuredCsv.DeserializeFloatArray(text, typeof(Vector));
		return new Vector(gpu, document.Data, document.Columns, cache: document.Data.Length > 0);
	}

	string IFormatter<Vector3>.Serialize(Vector3 vector, int flags)
	{
		ArgumentNullException.ThrowIfNull(vector);
		vector.SyncCPU();
		return StructuredCsv.SerializeFloatArray(typeof(Vector3), vector.Columns, vector.ToArray());
	}

	Vector3 IFormatter<Vector3>.Deserialize(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		FloatArrayDocument document = StructuredCsv.DeserializeFloatArray(text, typeof(Vector3));
		StructuredIoValidation.ValidateVector3Layout(document.Columns, document.Data.Length);
		return new Vector3(gpu, document.Data, cache: document.Data.Length > 0);
	}

	string IFormatter<Mask>.Serialize(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);
		mask.SyncCPU();

		return flags switch
		{
			MaskSerializeFlags.Packed => StructuredCsv.SerializeMaskPacked(
				mask.Columns,
				mask.ElementCount,
				mask.ToWordArray()),
			MaskSerializeFlags.Bool => StructuredCsv.SerializeMaskBool(mask.Columns, mask.ToBoolArray()),
			_ => throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unsupported mask serialize flags."),
		};
	}

	Mask IFormatter<Mask>.Deserialize(GPU gpu, string text) => StructuredCsv.DeserializeMask(gpu, text);
}
