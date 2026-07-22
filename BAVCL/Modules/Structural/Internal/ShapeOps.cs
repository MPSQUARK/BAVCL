using System;
using System.Linq;
using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Structural;

internal static class ShapeOpsCore
{
	internal static Vector Append(Vector left, Vector right) =>
		new(left.Gpu, left.RetrieveReadOnlySpan().ToArray().Concat(right.RetrieveReadOnlySpan().ToArray()).ToArray(), left.Columns);

	internal static void AppendInPlace(Vector vectorA, Vector vectorB)
	{
		using (vectorA.CpuScopeAndSync())
		{
			ReadOnlySpan<float> left = vectorA.GetCpuReadOnlySpan();
			ReadOnlySpan<float> right = vectorB.RetrieveReadOnlySpan();
			vectorA.Value = [.. left, .. right];
			vectorA.Length = vectorA.Value.Length;
		}
	}

	internal static Vector Prepend(Vector left, Vector right) => Append(right, left);

	internal static Vector Concat(Vector left, Vector right, char axis = 'r', bool warp = false)
	{
		Vector copy = left.Copy();
		ConcatInPlace(copy, right, axis, warp);
		return copy;
	}

	internal static Vector ConcatInPlace(Vector vector, Vector other, char axis = 'r', bool warp = false)
	{
		if (axis == 'r')
		{
			AppendInPlace(vector, other);
			return vector;
		}

		if (vector.Columns > 1 && other.Columns > 1)
		{
			if ((vector.RowCount() != other.RowCount()) && (vector.RowCount() != other.Columns))
			{
				throw new Exception(
					$"Vectors CANNOT be appended. " +
					$"This Vector has the shape ({vector.RowCount()},{vector.Columns}). " +
					$"The 2D Vector being appended has the shape ({other.RowCount()},{other.Columns})");
			}

			if (vector.RowCount() == other.Columns)
			{
				if (!warp)
					TransposeInPlace(other);

				if (warp && (other.Length % vector.RowCount() == 0))
					other.Columns = other.Length / vector.RowCount();
			}
		}

		if (other.Is1D())
		{
			if (other.Length % vector.RowCount() != 0)
			{
				throw new Exception($"Vectors CANNOT be appended. " +
					$"This array has shape ({vector.RowCount()},{vector.Columns}), 1D vector being appended has {other.Length} Length");
			}

			other.Columns = other.Length / vector.RowCount();
		}

		Vector output = new(vector.Gpu, other.Length + vector.Length);

		using (GpuScope.Begin(output, vector, other))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer(),
				buffer3 = other.GetBuffer();

			vector.Gpu.appendKernel(vector.Gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View, vector.Columns, other.Columns);
			vector.Gpu.accelerator.Synchronize();
		}

		vector.Columns += other.Columns;

		return TransferBuffer(vector, output);
	}

	internal static Vector Merge(Vector left, Vector right) =>
		new(left.Gpu, left.RetrieveReadOnlySpan().ToArray().Union(right.RetrieveReadOnlySpan().ToArray()).ToArray(), left.Columns);

	internal static void MergeInPlace(Vector vector, Vector vectorB)
	{
		using (vector.CpuScopeAndSync())
		{
			ReadOnlySpan<float> left = vector.GetCpuReadOnlySpan();
			ReadOnlySpan<float> right = vectorB.RetrieveReadOnlySpan();
			vector.Value = left.ToArray().Union(right.ToArray()).ToArray();
			vector.Length = vector.Value.Length;
		}
	}

	internal static Vector Reverse(Vector vector) =>
		new(vector.Gpu, vector.ToArray().Reverse().ToArray(), vector.Columns);

	internal static void ReverseInPlace(Vector vector)
	{
		using (vector.CpuScopeAndSync())
		{
			ReadOnlySpan<float> src = vector.GetCpuReadOnlySpan();
			float[] reversed = new float[src.Length];
			for (int i = 0; i < src.Length; i++)
				reversed[i] = src[src.Length - 1 - i];
			vector.Value = reversed;
			vector.Length = vector.Value.Length;
		}
	}

	internal static Vector ReverseX(Vector vector)
	{
		Vector copy = vector.Copy();
		ReverseXInPlace(copy);
		return copy;
	}

	internal static Vector ReverseXInPlace(Vector vector)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.reverseKernel(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent >> 1, buffer.View);
			vector.Gpu.accelerator.Synchronize();
		}

		return vector;
	}

	internal static Vector Transpose(Vector vector)
	{
		if (vector.Is1D() || vector.Columns >= vector.Length) { throw new Exception("Cannot transpose 1D Vector"); }

		Vector output = new(vector.Gpu, vector.Length, vector.RowCount());

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			vector.Gpu.transposekernel(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, vector.Columns);
			vector.Gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector TransposeInPlace(Vector vector) =>
		TransferBuffer(vector, Transpose(vector), true);

	internal static Vector TransferBuffer(Vector inheritee, Vector temp, bool incColumns = false)
	{
		inheritee.Gpu.FreeBuffer(inheritee.ID);
		Residence transferred = temp.Residence;
		inheritee.ID = temp.ID;
		inheritee.Value = temp.Value;
		inheritee.Length = temp.Length;
		if (incColumns) { inheritee.Columns = temp.Columns; }
		inheritee.SetResidence(transferred);

		temp.ID = 0;
		temp.SetResidence(Residence.Cpu);
		return inheritee;
	}

	internal static float[] GetRowAsArray(Vector vector, int row) =>
		vector.RetrieveReadOnlySpan().Slice(row * vector.Columns, vector.Columns).ToArray();

	internal static float[] GetRowAsArray(Vector vector, int row, bool noSync) =>
		vector.GetCpuReadOnlySpan().Slice(row * vector.Columns, vector.Columns).ToArray();

	internal static Vector GetRowAsVector(Vector vector, int row) =>
		new(vector.Gpu, GetRowAsArray(vector, row), 0);

	internal static Vector GetColumnAsVector(Vector vector, int column)
	{
		int[] select = [column, vector.Columns];
		Vector output = new(vector.Gpu, vector.RowCount());

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			MemoryBuffer1D<int, Stride1D.Dense> buffer3 = vector.Gpu.accelerator.Allocate1D(select);

			vector.Gpu.getSliceKernel(vector.Gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View);
			vector.Gpu.accelerator.Synchronize();
			buffer3.Dispose();
		}

		return output;
	}

	internal static float[] GetColumnAsArray(Vector vector, int column) =>
		GetColumnAsVector(vector, column).ToArray();

	internal static Vector GetSliceAsVector(Vector vector, int row_col_index, Axis axis)
	{
		if (vector.Is1D())
			throw new Exception("Input Vector cannot be 1D");

		return axis switch
		{
			Axis.Row => GetRowAsVector(vector, row_col_index),
			Axis.Column => GetColumnAsVector(vector, row_col_index),
			_ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
		};
	}

	internal static float[] GetSliceAsArray(Vector vector, int row_col_index, Axis axis)
	{
		if (vector.Is1D())
			throw new Exception("Input Vector cannot be 1D");

		return axis switch
		{
			Axis.Row => GetRowAsArray(vector, row_col_index),
			Axis.Column => GetColumnAsArray(vector, row_col_index),
			_ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
		};
	}
}
