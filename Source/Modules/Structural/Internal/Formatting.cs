using System;
using System.Text;
using BAVCL.Types;

namespace BAVCL.Modules.Structural;

internal static class FormattingCore
{
	internal static string ToCsv<T>(VectorBase<T> vector) where T : unmanaged
	{
		ReadOnlySpan<T> data = vector.RetrieveReadOnlySpan();
		var stringBuilder = new StringBuilder();
		int columns = vector.Columns;
		int length = vector.Length;

		if (columns == 0)
		{
			for (int i = 0; i < length; i++)
				stringBuilder.Append($"{data[i]},");

			return stringBuilder.ToString();
		}

		stringBuilder.Append($"{data[0]},");

		for (int i = 1, col = 0; i < length; i++, col++)
		{
            if (col == columns)
            {
                stringBuilder.AppendLine();
                col = 0;
            }

            stringBuilder.Append($"{data[i]},");
		}

		return stringBuilder.ToString();
	}

	internal static string ToStr(float[] arr, byte decimalplaces = 2)
	{
		(float min, float max, bool hasinfinity) = Util.MinMaxInf(arr);

		bool hasnegative = min < 0f;

		int high = max.ToString().Length;
		int low = hasnegative ? min.ToString().Length - 1 : min.ToString().Length;

		int digits = high > low ? high : low;

		string format = $"F{decimalplaces}";

		char[] Template = new char[digits + decimalplaces + 7];

		Template[0] = '|';
		Template[1] = ' ';
		Template[2] = ' ';
		Template[^4] = ' ';
		Template[^3] = ' ';
		Template[^2] = '|';
		Template[^1] = '\n';

		StringBuilder stringBuilder = new();
		char[] clear = new string(' ', Template.Length - 6).ToCharArray();
		int _diff = digits + 4 + decimalplaces;

		if (hasinfinity)
		{
			string inf = new(' ', digits - 3);
			string afterinf = new(' ', decimalplaces + 1);
			string nan = new(' ', decimalplaces);

			for (int i = 0; i < arr.Length; i++)
			{
				Template[2] = arr[i] < 0f ? '-' : ' ';

				if (float.IsFinite(arr[i]))
				{
					clear.CopyTo(Template, 3);
					string val = Math.Abs(arr[i]).ToString(format);
					val.CopyTo(0, Template, _diff - val.Length, val.Length);
					stringBuilder.Append(Template);
					continue;
				}

				if (float.IsPositiveInfinity(arr[i]))
				{
					stringBuilder.Append($"|  {inf}INF{afterinf} |\n");
					continue;
				}

				if (float.IsNaN(arr[i]))
				{
					stringBuilder.Append($"|  {inf}NaN{afterinf} |\n");
					continue;
				}

				if (float.IsNegativeInfinity(arr[i]))
				{
					stringBuilder.Append($"| -{inf}INF{nan}  |\n");
					continue;
				}
			}
			return stringBuilder.ToString();
		}

		if (hasnegative)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				Template[2] = arr[i] < 0f ? '-' : ' ';

				clear.CopyTo(Template, 3);
				string val = Math.Abs(arr[i]).ToString(format);
				val.CopyTo(0, Template, _diff - val.Length, val.Length);

				stringBuilder.Append(Template);
			}

			return stringBuilder.ToString();
		}

		for (int i = 0; i < arr.Length; i++)
		{
			clear.CopyTo(Template, 3);
			string val = arr[i].ToString(format);
			val.CopyTo(0, Template, _diff - val.Length, val.Length);
			stringBuilder.Append(Template);
		}

		return stringBuilder.ToString();
	}

	internal static string ToStr(Vector vector, byte decimalplaces = 2)
	{
		ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();

		int layoutColumns = vector.Is1D() ? vector.Length : vector.Columns;

		(float min, float max, bool hasinfinity) = Util.MinMaxInf(data);

		bool hasnegative = min < 0f;

		int high = max.ToString().Length;
		int low = hasnegative ? min.ToString().Length - 1 : min.ToString().Length;

		int digits = high > low ? high : low;

		string format = $"F{decimalplaces}";

		char[] Template = new char[digits + decimalplaces + 6];

		Template[0] = '|';
		Template[1] = ' ';
		Template[2] = ' ';
		Template[^3] = ' ';
		Template[^2] = ' ';
		Template[^1] = '|';

		StringBuilder stringBuilder = new();
		char[] clear = new string(' ', Template.Length - 6).ToCharArray();
		int _diff = digits + 4 + decimalplaces;

		if (hasinfinity)
		{
			string inf = new(' ', digits - 3);
			string afterinf = new(' ', decimalplaces + 1);
			string nan = new(' ', decimalplaces);

			for (int i = 0, col = 0; i < vector.Length; i++, col++)
			{
				if (col == layoutColumns)
				{
					stringBuilder.AppendLine();
					col = 0;
				}

				Template[2] = data[i] < 0f ? '-' : ' ';

				if (float.IsFinite(data[i]))
				{
					clear.CopyTo(Template, 3);
					string val = Math.Abs(data[i]).ToString(format);
					val.CopyTo(0, Template, _diff - val.Length, val.Length);
					stringBuilder.Append(Template);
					continue;
				}

				if (float.IsPositiveInfinity(data[i]))
				{
					stringBuilder.Append($"|  {inf}INF{afterinf} |");
					continue;
				}

				if (float.IsNaN(data[i]))
				{
					stringBuilder.Append($"|  {inf}NaN{afterinf} |");
					continue;
				}

				if (float.IsNegativeInfinity(data[i]))
				{
					stringBuilder.Append($"| -{inf}INF{nan}  |");
					continue;
				}
			}
			return stringBuilder.ToString();
		}

		if (hasnegative)
		{
			for (int i = 0, col = 0; i < vector.Length; i++, col++)
			{
				if (col == layoutColumns)
				{
					stringBuilder.AppendLine();
					col = 0;
				}

				Template[2] = data[i] < 0f ? '-' : ' ';

				clear.CopyTo(Template, 3);
				string val = Math.Abs(data[i]).ToString(format);
				val.CopyTo(0, Template, _diff - val.Length, val.Length);

				stringBuilder.Append(Template);
			}

			return stringBuilder.ToString();
		}

		for (int i = 0, col = 0; i < vector.Length; i++, col++)
		{
			if (col == layoutColumns)
			{
				stringBuilder.AppendLine();
				col = 0;
			}

			clear.CopyTo(Template, 3);
			string val = data[i].ToString(format);
			val.CopyTo(0, Template, _diff - val.Length, val.Length);

			stringBuilder.Append(Template);
		}

		return stringBuilder.ToString();
	}

	internal static string ToStr(Mask mask){
		if (mask.Columns > 1)
			return ToStr2D(mask);

		char[] strBuffer = new char[mask.ElementCount * 6];
		ReadOnlySpan<int> data = mask.RetrieveReadOnlySpan();

		char[] template = ['|', ' ', ' ', ' ', '|', '\n'];           

        // Use tamplate to fill the 2nd index with the value of the mask element
        // then copy the template to the strBuffer at the correct index
        for (int i = 0; i < mask.ElementCount; i++)
        {
            // isolate the index of the densely packed int32 word
            int bit = (data[i >> 5] >> (i & 31)) & 1;
            template[2] = (char)('0' + bit);
            template.CopyTo(strBuffer, i * 6);
        }

        return new string(strBuffer);
    }

	private static string ToStr2D(Mask mask){
        StringBuilder sb = new();

		char[] template = ['|', ' ', ' ', ' ', '|'];
        ReadOnlySpan<int> data = mask.RetrieveReadOnlySpan();
        for (int i = 0, col = 0; i < mask.ElementCount; i++, col++)
        {
			if (col == mask.Columns)
			{
                sb.AppendLine();
                col = 0;
            }

            int bit = (data[i >> 5] >> (i & 31)) & 1;
            template[2] = (char)('0' + bit);
            sb.Append(template);
        }

        return sb.ToString();
    }

	internal static void Print(Mask mask) => Console.WriteLine(ToStr(mask));

    internal static void Print(float value, byte decimalplaces = 2) =>
		Console.WriteLine(value.ToString($"F{decimalplaces}"));

	internal static void Print(float[] arr, byte decimalplaces = 2)
	{
		Console.WriteLine();
		Console.WriteLine(ToStr(arr, decimalplaces));
	}

	internal static void Print(float[,] arr, byte decimalplaces = 2) =>
		throw new NotImplementedException();

	internal static void Print(double value, byte decimalplaces = 2) =>
		Console.WriteLine(value.ToString($"F{decimalplaces}"));

	internal static void Print(double[] arr, byte decimalplaces = 2) =>
		throw new NotImplementedException();

	internal static void Print(double[,] arr, byte decimalplaces = 2) =>
		throw new NotImplementedException();

	internal static void Print(int value) =>
		Console.WriteLine(value.ToString());

	internal static void Print(int[] arr) =>
		throw new NotImplementedException();

	internal static void Print(int[,] arr) =>
		throw new NotImplementedException();

	internal static void Print(uint value) =>
		Console.WriteLine(value.ToString());

	internal static void Print(uint[] arr)
	{
		for (int i = 0; i < arr.Length - 1; i++)
			Console.Write($"{arr[i]},");
		Console.Write($"{arr[^1]}\n");
	}

	internal static void Print(uint[,] arr) =>
		throw new NotImplementedException();

	internal static void Print(long value) =>
		Console.WriteLine(value.ToString());

	internal static void Print(long[] arr) =>
		throw new NotImplementedException();

	internal static void Print(long[,] arr) =>
		throw new NotImplementedException();
}
