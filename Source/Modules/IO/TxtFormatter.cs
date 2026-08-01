using System;
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
	ISingleton<TxtFormatter>
{
	public static TxtFormatter Default { get; } = new();

	static TxtFormatter ISingleton<TxtFormatter>.Default => Default;

	TxtFormatter() { }

	public string Extension => ".txt";

	string IFormatter<Vector>.Serialize(Vector vector, int flags)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return vector.ToString()!;
	}

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		(float[] values, int columns) = TxtParsing.ParseFloatGrid(text);
		return new Vector(gpu, values, columns, cache: values.Length > 0);
	}

	string IFormatter<Vector3>.Serialize(Vector3 vector, int flags)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return vector.ToString()!;
	}

	Vector3 IFormatter<Vector3>.Deserialize(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		(float[] values, int columns) = TxtParsing.ParseFloatGrid(text, requiredColumns: 3);
		StructuredIoValidation.ValidateVector3Layout(columns, values.Length);

		return new Vector3(gpu, values, cache: values.Length > 0);
	}

	string IFormatter<Mask>.Serialize(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);
		return mask.ToStr();
	}

	Mask IFormatter<Mask>.Deserialize(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		(bool[] values, int columns) = TxtParsing.ParseBoolGrid(text);
		return new Mask(gpu, values, columns);
	}
}
