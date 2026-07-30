using System;
using BAVCL.Geometric;
using BAVCL.Modules.Structural;
using BAVCL.Core.Interfaces;
using BAVCL.Types;

namespace BAVCL.Modules.IO;

/// <summary>Plain-text output via type formatting (write only).</summary>
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

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text) =>
		throw new NotSupportedException("Reading Vector from TXT is not supported.");

	string IFormatter<Vector3>.Serialize(Vector3 vector, int flags)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return vector.ToString()!;
	}

	Vector3 IFormatter<Vector3>.Deserialize(GPU gpu, string text) =>
		throw new NotSupportedException("Reading Vector3 from TXT is not supported.");

	string IFormatter<Mask>.Serialize(Mask mask, int flags)
	{
		ArgumentNullException.ThrowIfNull(mask);
		return mask.ToStr();
	}

	Mask IFormatter<Mask>.Deserialize(GPU gpu, string text) =>
		throw new NotSupportedException("Reading Mask from TXT is not supported.");
}
