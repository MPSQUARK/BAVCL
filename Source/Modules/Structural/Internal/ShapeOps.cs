using System;
using System.Linq;
using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Structural;

internal static class ShapeOpsCore
{
	const string ConcatColumnMessage = "Column-axis concatenation requires ConcatColumnX or ConcatColumnXIP.";

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

	internal static Vector Concat(Vector left, Vector right, ConcatAxis axis = ConcatAxis.Row, bool warp = false)
	{
		Vector copy = left.Copy();
		ConcatInPlace(copy, right, axis, warp);
		return copy;
	}

	internal static Vector ConcatInPlace(Vector vector, Vector other, ConcatAxis axis = ConcatAxis.Row, bool warp = false)
	{
		if (axis == ConcatAxis.Column)
			throw new Exception(ConcatColumnMessage);

		AppendInPlace(vector, other);
		return vector;
	}

	internal static Vector ConcatColumnX(Vector left, Vector right, bool warp = false)
	{
		Vector copy = left.Copy();
		ConcatColumnXInPlace(copy, right, warp);
		return copy;
	}

	internal static Vector ConcatColumnXInPlace(Vector vector, Vector other, bool warp = false)
	{
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
					TransposeXInPlace(other);

				if (warp && (other.Length % vector.RowCount() == 0))
					other.Columns = other.Length / vector.RowCount();
			}
		}

		if (other.Is1DRowVector())
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

			vector.Gpu.appendKernel(vector.Gpu.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View, vector.Columns, other.Columns);
			vector.Gpu.Synchronize();
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

	internal static Vector TransposeX(Vector vector)
	{
		if (vector.Is1DRowVector() || vector.Columns >= vector.Length) { throw new Exception("Cannot transpose 1D Vector"); }

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

	internal static Vector TransposeXInPlace(Vector vector) =>
		TransferBuffer(vector, TransposeX(vector), true);

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

	internal static Vector GetColumnAsVectorX(Vector vector, int column)
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
		GetColumnAsVectorX(vector, column).ToArray();

	internal static Vector GetSliceAsVector(Vector vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		if (axis == Axis.Column)
			throw new Exception("Column-axis slice extraction requires GetSliceAsVectorX.");

		return axis switch
		{
			Axis.Row => GetRowAsVector(vector, row_col_index),
			_ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
		};
	}

	internal static Vector GetSliceAsVectorX(Vector vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		return axis switch
		{
			Axis.Column => GetColumnAsVectorX(vector, row_col_index),
			_ => throw new Exception("GetSliceAsVectorX supports column axis only. Use GetSliceAsVector for row axis."),
		};
	}

	internal static float[] GetSliceAsArray(Vector vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		if (axis == Axis.Column)
			throw new Exception("Column-axis slice extraction requires GetSliceAsArrayX.");

		return axis switch
		{
			Axis.Row => GetRowAsArray(vector, row_col_index),
			_ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
		};
	}

	internal static float[] GetSliceAsArrayX(Vector vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		return axis switch
		{
			Axis.Column => GetColumnAsArray(vector, row_col_index),
			_ => throw new Exception("GetSliceAsArrayX supports column axis only. Use GetSliceAsArray for row axis."),
		};
	}

	internal static VectorInt Append(VectorInt left, VectorInt right) =>
		new(left.Gpu, left.RetrieveReadOnlySpan().ToArray().Concat(right.RetrieveReadOnlySpan().ToArray()).ToArray(), left.Columns);

	internal static VectorInt Prepend(VectorInt left, VectorInt right) => Append(right, left);

	internal static void AppendInPlace(VectorInt vectorA, VectorInt vectorB)
	{
		using (vectorA.CpuScopeAndSync())
		{
			ReadOnlySpan<int> left = vectorA.GetCpuReadOnlySpan();
			ReadOnlySpan<int> right = vectorB.RetrieveReadOnlySpan();
			vectorA.Value = [.. left, .. right];
			vectorA.Length = vectorA.Value.Length;
		}
	}

	internal static VectorInt Concat(VectorInt left, VectorInt right, ConcatAxis axis = ConcatAxis.Row, bool warp = false)
	{
		VectorInt copy = left.Copy();
		ConcatInPlace(copy, right, axis, warp);
		return copy;
	}

	internal static VectorInt ConcatInPlace(VectorInt vector, VectorInt other, ConcatAxis axis = ConcatAxis.Row, bool warp = false)
	{
		if (axis == ConcatAxis.Column)
			throw new Exception(ConcatColumnMessage);

		AppendInPlace(vector, other);
		return vector;
	}

	internal static VectorInt ConcatColumnX(VectorInt left, VectorInt right, bool warp = false)
	{
		VectorInt copy = left.Copy();
		ConcatColumnXInPlace(copy, right, warp);
		return copy;
	}

	internal static VectorInt ConcatColumnXInPlace(VectorInt vector, VectorInt other, bool warp = false)
	{
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
					TransposeXInPlace(other);

				if (warp && (other.Length % vector.RowCount() == 0))
					other.Columns = other.Length / vector.RowCount();
			}
		}

		if (other.Is1DRowVector())
		{
			if (other.Length % vector.RowCount() != 0)
			{
				throw new Exception($"Vectors CANNOT be appended. " +
					$"This array has shape ({vector.RowCount()},{vector.Columns}), 1D vector being appended has {other.Length} Length");
			}

			other.Columns = other.Length / vector.RowCount();
		}

		VectorInt output = new(vector.Gpu, other.Length + vector.Length);

		using (GpuScope.Begin(output, vector, other))
		{
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer(),
				buffer3 = other.GetBuffer();

			vector.Gpu.appendIntKernel(vector.Gpu.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View, vector.Columns, other.Columns);
			vector.Gpu.Synchronize();
		}

		vector.Columns += other.Columns;

		return TransferBuffer(vector, output);
	}

	internal static VectorInt Merge(VectorInt left, VectorInt right) =>
		new(left.Gpu, left.RetrieveReadOnlySpan().ToArray().Union(right.RetrieveReadOnlySpan().ToArray()).ToArray(), left.Columns);

	internal static void MergeInPlace(VectorInt vector, VectorInt vectorB)
	{
		using (vector.CpuScopeAndSync())
		{
			ReadOnlySpan<int> left = vector.GetCpuReadOnlySpan();
			ReadOnlySpan<int> right = vectorB.RetrieveReadOnlySpan();
			vector.Value = left.ToArray().Union(right.ToArray()).ToArray();
			vector.Length = vector.Value.Length;
		}
	}

	internal static void ReverseInPlace(VectorInt vector)
	{
		using (vector.CpuScopeAndSync())
		{
			ReadOnlySpan<int> src = vector.GetCpuReadOnlySpan();
			int[] reversed = new int[src.Length];
			for (int i = 0; i < src.Length; i++)
				reversed[i] = src[src.Length - 1 - i];
			vector.Value = reversed;
			vector.Length = vector.Value.Length;
		}
	}

	internal static VectorInt Reverse(VectorInt vector) =>
		new(vector.Gpu, vector.ToArray().Reverse().ToArray(), vector.Columns);

	internal static VectorInt ReverseX(VectorInt vector)
	{
		VectorInt copy = vector.Copy();
		ReverseXInPlace(copy);
		return copy;
	}

	internal static VectorInt ReverseXInPlace(VectorInt vector)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.reverseIntKernel(vector.Gpu.DefaultStream, buffer.IntExtent >> 1, buffer.View);
			vector.Gpu.Synchronize();
		}

		return vector;
	}

	internal static VectorInt TransposeX(VectorInt vector)
	{
		if (vector.Is1DRowVector() || vector.Columns >= vector.Length) { throw new Exception("Cannot transpose 1D Vector"); }

		VectorInt output = new(vector.Gpu, vector.Length, vector.RowCount());

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			vector.Gpu.transposeIntKernel(vector.Gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, vector.Columns);
			vector.Gpu.Synchronize();
		}

		return output;
	}

	internal static VectorInt TransposeXInPlace(VectorInt vector) =>
		TransferBuffer(vector, TransposeX(vector), true);

	internal static VectorInt TransferBuffer(VectorInt inheritee, VectorInt temp, bool incColumns = false)
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

	internal static int[] GetRowAsArray(VectorInt vector, int row) =>
		vector.RetrieveReadOnlySpan().Slice(row * vector.Columns, vector.Columns).ToArray();

	internal static VectorInt GetRowAsVector(VectorInt vector, int row) =>
		new(vector.Gpu, GetRowAsArray(vector, row), 0);

	internal static VectorInt GetColumnAsVectorX(VectorInt vector, int column)
	{
		int[] select = [column, vector.Columns];
		VectorInt output = new(vector.Gpu, vector.RowCount());

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			MemoryBuffer1D<int, Stride1D.Dense> buffer3 = vector.Gpu.accelerator.Allocate1D(select);

			vector.Gpu.getSliceIntKernel(vector.Gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View);
			vector.Gpu.accelerator.Synchronize();
			buffer3.Dispose();
		}

		return output;
	}

	internal static int[] GetColumnAsArray(VectorInt vector, int column) =>
		GetColumnAsVectorX(vector, column).ToArray();

	internal static VectorInt GetSliceAsVector(VectorInt vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		if (axis == Axis.Column)
			throw new Exception("Column-axis slice extraction requires GetSliceAsVectorX.");

		return axis switch
		{
			Axis.Row => GetRowAsVector(vector, row_col_index),
			_ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
		};
	}

	internal static VectorInt GetSliceAsVectorX(VectorInt vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		return axis switch
		{
			Axis.Column => GetColumnAsVectorX(vector, row_col_index),
			_ => throw new Exception("GetSliceAsVectorX supports column axis only. Use GetSliceAsVector for row axis."),
		};
	}

	internal static int[] GetSliceAsArray(VectorInt vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		if (axis == Axis.Column)
			throw new Exception("Column-axis slice extraction requires GetSliceAsArrayX.");

		return axis switch
		{
			Axis.Row => GetRowAsArray(vector, row_col_index),
			_ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
		};
	}

	internal static int[] GetSliceAsArrayX(VectorInt vector, int row_col_index, Axis axis)
	{
		if (vector.Is1DRowVector())
			throw new Exception("Input Vector cannot be 1D");

		return axis switch
		{
			Axis.Column => GetColumnAsArray(vector, row_col_index),
			_ => throw new Exception("GetSliceAsArrayX supports column axis only. Use GetSliceAsArray for row axis."),
		};
	}
}
