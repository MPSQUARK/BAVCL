using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BAVCL.Modules.IO.Internal;

internal static partial class TxtParsing
{
	internal static (float[] Values, int Columns) ParseFloatGrid(string text, int? requiredColumns = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		List<string[]> rows = ExtractRows(text);
		if (rows.Count == 0)
			return ([], requiredColumns ?? 0);

		int columns = InferFloatColumns(rows, requiredColumns);

		if (requiredColumns is int required && columns != required)
			throw new FormatException($"TXT column count {columns} does not match expected {required}.");

		var values = new List<float>(rows.Count * Math.Max(columns, rows[0].Length));
		AppendGridRows(rows, columns, values, ParseFloatCell);

		return (values.ToArray(), columns);
	}

	internal static (bool[] Values, int Columns) ParseBoolGrid(string text)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		List<string[]> rows = ExtractRows(text);
		if (rows.Count == 0)
			throw new FormatException("TXT mask must contain at least one value.");

		int columns = InferBoolColumns(rows);
		var values = new List<bool>();
		AppendGridRows(rows, columns, values, cell => BoolIoParsing.ParseTxtCell(cell.Trim()));

		if (values.Count == 0)
			throw new FormatException("TXT mask must contain at least one value.");

		return (values.ToArray(), columns);
	}

	static void AppendGridRows<T>(List<string[]> rows, int columns, List<T> values, Func<string, T> parseCell)
	{
		for (int row = 0; row < rows.Count; row++)
		{
			if (columns != 0 && rows[row].Length != columns)
				throw new FormatException($"TXT row {row + 1} has {rows[row].Length} cells; expected {columns}.");

			foreach (string cell in rows[row])
				values.Add(parseCell(cell));
		}
	}

	static int InferFloatColumns(List<string[]> rows, int? requiredColumns = null)
	{
		if (requiredColumns is int required)
			return required;

		// 1D row (columns == 0): ToStr writes every element on one line with no row breaks.
		if (rows.Count == 1)
			return 0;

		// Column vector (columns == 1): ToStr inserts a newline before each element.
		if (rows.All(row => row.Length == 1))
			return 1;

		return rows[0].Length;
	}

	static int InferBoolColumns(List<string[]> rows)
	{
		if (rows.Count == 1)
			return rows[0].Length == 1 ? 0 : rows[0].Length;

		if (rows.All(row => row.Length == 1))
			return 0;

		return rows[0].Length;
	}

	static List<string[]> ExtractRows(string text)
	{
		string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
		string[] lines = normalized.Split('\n', StringSplitOptions.None);
		var rows = new List<string[]>();

		foreach (string line in lines)
		{
			if (string.IsNullOrWhiteSpace(line))
				continue;

			string[] cells = ExtractPipeCells(line);
			if (cells.Length == 0)
				continue;

			rows.Add(cells);
		}

		return rows;
	}

	static string[] ExtractPipeCells(string line)
	{
		var cells = new List<string>();
		foreach (Match match in PipeCellPattern().Matches(line))
		{
			string inner = match.Groups[1].Value;
			if (inner.Length > 0)
				cells.Add(inner);
		}

		return cells.ToArray();
	}

	static float ParseFloatCell(string cell)
	{
		string trimmed = cell.Trim();

		if (trimmed.Contains("NaN", StringComparison.OrdinalIgnoreCase))
			return float.NaN;

		if (trimmed.Contains("INF", StringComparison.OrdinalIgnoreCase))
		{
			bool negative = trimmed.StartsWith('-')
				|| trimmed.Contains("-INF", StringComparison.OrdinalIgnoreCase);

			return negative ? float.NegativeInfinity : float.PositiveInfinity;
		}

		trimmed = trimmed.Replace(" ", string.Empty);
		if (!float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
			throw new FormatException($"TXT cell '{cell}' is not a valid float.");

		return value;
	}

	[GeneratedRegex(@"\|\s*([^|]*?)\s*\|")]
	private static partial Regex PipeCellPattern();
}
