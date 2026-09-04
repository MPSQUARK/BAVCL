using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BAVCL.Core.Exceptions;
using BAVCL.Geometric;
using BAVCL.Modules.GpuOps;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Geometric;

internal static class Vector3Geometry
{
	internal static Vector MagnitudeX(Vector3 left, Vector3 right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(MagnitudeX), left.Length, right.Length);

		return left.VOPX(right, Operations.magnitude);
	}

	internal static Vector MagnitudeX(Vector3 vector) =>
		vector.VOPX(Operations.magnitude);

	internal static Vector DistanceX(Vector3 left, Vector3 right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(DistanceX), left.Length, right.Length);

		return left.VOPX(right, Operations.distance);
	}

	internal static Vector DotX(Vector3 left, Vector3 right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(DotX), left.Length, right.Length);

		return left.VOPX(right, Operations.multiply);
	}

	internal static Vector3 CrossX(Vector3 left, Vector3 right)
	{
		if (left.Length != right.Length)
			throw new Exception($"Cannot Cross Product two Vector3's together of different lengths. {left.Length} != {right.Length}");

		GPU gpu = left.Gpu;
		Vector3 output = new(gpu, left.Length);

		using (GpuScope.Begin(output, left, right))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = left.GetBuffer(),
				buffer3 = right.GetBuffer();

			gpu.crossKernel(gpu.accelerator.DefaultStream, left.Length / 3, buffer.View, buffer2.View, buffer3.View);
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector3 NormaliseX(Vector3 vector)
	{
		GPU gpu = vector.Gpu;
		Vector3 output = new(gpu, vector.Length);

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.normaliseKernel(gpu.accelerator.DefaultStream, vector.Length / 3, buffer.View, buffer2.View);
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector3 AccessRow(Vector3 vector, int vertRow)
	{
		ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
		float[] row = data.Slice(vertRow + vertRow + vertRow, 3).ToArray();
		return new Vector3(vector.Gpu, row);
	}

	internal static Vector3 Concat(Vector3 left, Vertex vertA) =>
		left.Copy().ConcatIP(vertA);

	internal static Vector3 Concat(Vector3 left, Vertex[] vertices) =>
		left.Copy().ConcatIP(vertices);

	internal static Vector3 Concat(Vector3 left, List<Vertex> vertices) =>
		left.Copy().ConcatIP(vertices);

	internal static Vector3 Concat(Vector3 left, Vector3 right) =>
		left.Copy().ConcatIP(right);

	internal static Vector3 Concat(Vector3 left, Vector3[] vectors) =>
		left.Copy().ConcatIP(vectors);

	internal static Vector3 Concat(Vector3 left, List<Vector3> vectors) =>
		left.Copy().ConcatIP(vectors);

	internal static Vector3 ConcatIP(Vector3 vector, Vertex vertA)
	{
		using (vector.CpuScopeAndSync())
		{
			ReadOnlySpan<float> left = vector.RetrieveReadOnlySpan();
			vector.Value = [..left, vertA.X, vertA.Y, vertA.Z];
            vector.Length = vector.Value.Length;
		}

		return vector;
	}

	internal static Vector3 ConcatIP(Vector3 vector, Vertex[] vertices)
	{
		using (vector.CpuScopeAndSync())
		{
            vector.Value = vertices.Aggregate(vector.RetrieveReadOnlySpan().ToArray(), (current, vert) =>
				[.. current, vert.X, vert.Y, vert.Z]);
			vector.Length = vector.Value.Length;
		}

		return vector;
	}

	internal static Vector3 ConcatIP(Vector3 vector, List<Vertex> vertices)
	{
		using (vector.CpuScopeAndSync())
		{
			ReadOnlySpan<float> left = vector.RetrieveReadOnlySpan();
			vector.Value = vertices.Aggregate(left.ToArray(), (current, vert) =>
				[.. current, vert.X, vert.Y, vert.Z]);
			vector.Length = vector.Value.Length;
		}

		return vector;
	}

	internal static Vector3 ConcatIP(Vector3 vector, Vector3 other)
	{
		using (vector.CpuScopeAndSync())
		{
			ReadOnlySpan<float> left = vector.RetrieveReadOnlySpan();
			ReadOnlySpan<float> right = other.RetrieveReadOnlySpan();
			vector.Value = left.ToArray().Concat(right.ToArray()).ToArray();
			vector.Length = vector.Value.Length;
		}

		return vector;
	}

	internal static Vector3 ConcatIP(Vector3 vector, Vector3[] vectors)
	{
		using (vector.CpuScopeAndSync())
		{
			float[] merged = vector.RetrieveReadOnlySpan().ToArray();
			for (int i = 0; i < vectors.Length; i++)
				merged = merged.Concat(vectors[i].RetrieveReadOnlySpan().ToArray()).ToArray();

			vector.Value = merged;
			vector.Length = vector.Value.Length;
		}

		return vector;
	}

	internal static Vector3 ConcatIP(Vector3 vector, List<Vector3> vectors)
	{
		using (vector.CpuScopeAndSync())
		{
			float[] merged = vector.RetrieveReadOnlySpan().ToArray();
			for (int i = 0; i < vectors.Count; i++)
				merged = [.. merged, .. vectors[i].RetrieveReadOnlySpan().ToArray()];

			vector.Value = merged;
			vector.Length = vector.Value.Length;
		}

		return vector;
	}

	internal static string Format(Vector3 vector3, byte decimalplaces = 2)
	{
		ReadOnlySpan<float> data = vector3.RetrieveReadOnlySpan();
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

			for (int i = 0, col = 0; i < vector3.Length; i++, col++)
			{
				if (col == vector3.Columns)
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
			for (int i = 0, col = 0; i < vector3.Length; i++, col++)
			{
				if (col == vector3.Columns)
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

		for (int i = 0, col = 0; i < vector3.Length; i++, col++)
		{
			if (col == vector3.Columns)
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
}
