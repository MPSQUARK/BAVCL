using System;
using System.Linq;
using System.Text.RegularExpressions;
using BAVCL.Core.Interfaces;

namespace BAVCL.Modules.IO;

/// <summary>CSV persistence for Vector (write and read).</summary>
public sealed partial class CsvFormatter : IFormatter<Vector>, ISingleton<CsvFormatter>
{
	public static CsvFormatter Default { get; } = new();

	static CsvFormatter ISingleton<CsvFormatter>.Default => Default;

	public string Extension => ".csv";

	string IFormatter<Vector>.Serialize(Vector vector, int flags)
	{
		ArgumentNullException.ThrowIfNull(vector);
		return vector.ToCSV();
	}

	Vector IFormatter<Vector>.Deserialize(GPU gpu, string text) => DeserializeVector(gpu, text);

	static Vector DeserializeVector(GPU gpu, string text)
	{
		ArgumentNullException.ThrowIfNull(text);

		int columns = ColumnsSelector().Match(text).ToString().Split(',').Length - 1;
		float[] values = Array.ConvertAll(
			text.Replace("\r\n", "").Split(',').ToArray()[..^1],
			float.Parse);

		return new Vector(gpu, values, columns);
	}

    [GeneratedRegex(@"^.*?(?=\n)")]
    private static partial Regex ColumnsSelector();
}
