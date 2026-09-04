using BAVCL.Core;
using BAVCL.Modules.Generators;
using BAVCL.Types;

namespace BAVCL.Modules.Structural;

public static class VectorStructural
{
	extension(float[])
	{
		public static string ToStr(float[] arr, byte decimalplaces = 2) =>
			FormattingCore.ToStr(arr, decimalplaces);

		public static void Print(float[] arr, byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(float)
	{
		public static void Print(float value, byte decimalplaces = 2) =>
			FormattingCore.Print(value, decimalplaces);
	}

	extension(float[,])
	{
		public static void Print(float[,] arr, byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(double)
	{
		public static void Print(double value, byte decimalplaces = 2) =>
			FormattingCore.Print(value, decimalplaces);
	}

	extension(double[])
	{
		public static void Print(double[] arr, byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(double[,])
	{
		public static void Print(double[,] arr, byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(int)
	{
		public static void Print(int value) =>
			FormattingCore.Print(value);
	}

	extension(int[])
	{
		public static void Print(int[] arr) =>
			FormattingCore.Print(arr);
	}

	extension(int[,])
	{
		public static void Print(int[,] arr) =>
			FormattingCore.Print(arr);
	}

	extension(uint)
	{
		public static void Print(uint value) =>
			FormattingCore.Print(value);
	}

	extension(uint[])
	{
		public static void Print(uint[] arr) =>
			FormattingCore.Print(arr);
	}

	extension(uint[,])
	{
		public static void Print(uint[,] arr) =>
			FormattingCore.Print(arr);
	}

	extension(long)
	{
		public static void Print(long value) =>
			FormattingCore.Print(value);
	}

	extension(long[])
	{
		public static void Print(long[] arr) =>
			FormattingCore.Print(arr);
	}

	extension(long[,])
	{
		public static void Print(long[,] arr) =>
			FormattingCore.Print(arr);
	}

	extension(Vector)
	{
		public static Vector Zeros(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.Zeros(gpu, length, columns);

		public static Vector Ones(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.Ones(gpu, length, columns);

		public static Vector Fill(GPU gpu, float value, int length, int columns = 0, bool cache = true) =>
			FactoriesCore.Fill(gpu, value, length, columns, cache);

		public static Vector Arange(GPU gpu, float startval, float endval, float interval, int columns = 0, bool cache = true) =>
			FactoriesCore.Arange(gpu, startval, endval, interval, columns, cache);

		public static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns = 0, bool cache = true) =>
			FactoriesCore.Linspace(gpu, startval, endval, steps, columns, cache);

		public static Vector Append(Vector left, Vector right) =>
			ShapeOpsCore.Append(left, right);

		public static Vector Prepend(Vector left, Vector right) =>
			ShapeOpsCore.Prepend(left, right);

		public static Vector Concat(Vector left, Vector right, ConcatAxis axis = ConcatAxis.Row, bool warp = false) =>
			ShapeOpsCore.Concat(left, right, axis, warp);

		public static Vector Merge(Vector left, Vector right) =>
			ShapeOpsCore.Merge(left, right);

		public static Vector ConcatColumnX(Vector left, Vector right, bool warp = false) =>
			ShapeOpsCore.ConcatColumnX(left, right, warp);

		public static Vector Reverse(Vector vector) =>
			ShapeOpsCore.Reverse(vector);

		public static Vector ReverseX(Vector vector) =>
			ShapeOpsCore.ReverseX(vector);

		public static Vector TransposeX(Vector vector) =>
			ShapeOpsCore.TransposeX(vector);

		public static Vector TransposeXIP(Vector vector) =>
			ShapeOpsCore.TransposeXInPlace(vector);

		public static Vector TransferBuffer(Vector inheritee, Vector temp, bool incColumns = false) =>
			ShapeOpsCore.TransferBuffer(inheritee, temp, incColumns);

		public static float[] GetRowAsArray(Vector vector, int row) =>
			ShapeOpsCore.GetRowAsArray(vector, row);

		public static Vector GetRowAsVector(Vector vector, int row) =>
			ShapeOpsCore.GetRowAsVector(vector, row);

		public static Vector GetColumnAsVectorX(Vector vector, int column) =>
			ShapeOpsCore.GetColumnAsVectorX(vector, column);

		public static float[] GetColumnAsArray(Vector vector, int column) =>
			ShapeOpsCore.GetColumnAsArray(vector, column);

		public static Vector GetSliceAsVector(Vector vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsVector(vector, row_col_index, axis);

		public static Vector GetSliceAsVectorX(Vector vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsVectorX(vector, row_col_index, axis);

		public static float[] GetSliceAsArray(Vector vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsArray(vector, row_col_index, axis);

		public static float[] GetSliceAsArrayX(Vector vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsArrayX(vector, row_col_index, axis);

		public static string ToStr(Vector vector, byte decimalplaces = 2) =>
			FormattingCore.ToStr(vector, decimalplaces);
	}

	extension(BAVCL.Geometric.Vector3)
	{
		public static BAVCL.Geometric.Vector3 Zeros(GPU gpu, int length) =>
			FactoriesCore.Zeros(gpu, length);

		public static BAVCL.Geometric.Vector3 Fill(GPU gpu, float value, int length) =>
			FactoriesCore.Fill(gpu, value, length);
	}

	extension(Mask)
	{
		public static string ToStr(Mask mask) => FormattingCore.ToStr(mask);
		public static void Print(Mask mask) => FormattingCore.Print(mask);

	}
}

/// <summary>
/// VectorInt static factories/shape ops. Split from <see cref="VectorStructural"/> because several
/// members (e.g. Zeros, Ones) share signatures with the float overloads once the return type is
/// excluded, which is a CS0111 conflict within one static class (see spec §2.6).
/// </summary>
public static class VectorIntStructural
{
	extension(VectorInt)
	{
		public static VectorInt Zeros(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.ZerosInt(gpu, length, columns);

		public static VectorInt Ones(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.OnesInt(gpu, length, columns);

		public static VectorInt Fill(GPU gpu, int value, int length, int columns = 0, bool cache = true) =>
			FactoriesCore.Fill(gpu, value, length, columns, cache);

		public static VectorInt Arange(GPU gpu, int startval, int endval, int interval, int columns = 0, bool cache = true) =>
			FactoriesCore.Arange(gpu, startval, endval, interval, columns, cache);

		public static VectorInt Linspace(GPU gpu, int startval, int endval, int steps, int columns = 0, bool cache = true) =>
			FactoriesCore.Linspace(gpu, startval, endval, steps, columns, cache);

		public static VectorInt Append(VectorInt left, VectorInt right) =>
			ShapeOpsCore.Append(left, right);

		public static VectorInt Prepend(VectorInt left, VectorInt right) =>
			ShapeOpsCore.Prepend(left, right);

		public static VectorInt Concat(VectorInt left, VectorInt right, ConcatAxis axis = ConcatAxis.Row, bool warp = false) =>
			ShapeOpsCore.Concat(left, right, axis, warp);

		public static VectorInt Merge(VectorInt left, VectorInt right) =>
			ShapeOpsCore.Merge(left, right);

		public static VectorInt ConcatColumnX(VectorInt left, VectorInt right, bool warp = false) =>
			ShapeOpsCore.ConcatColumnX(left, right, warp);

		public static VectorInt Reverse(VectorInt vector) =>
			ShapeOpsCore.Reverse(vector);

		public static VectorInt ReverseX(VectorInt vector) =>
			ShapeOpsCore.ReverseX(vector);

		public static VectorInt TransposeX(VectorInt vector) =>
			ShapeOpsCore.TransposeX(vector);

		public static VectorInt TransposeXIP(VectorInt vector) =>
			ShapeOpsCore.TransposeXInPlace(vector);

		public static VectorInt TransferBuffer(VectorInt inheritee, VectorInt temp, bool incColumns = false) =>
			ShapeOpsCore.TransferBuffer(inheritee, temp, incColumns);

		public static int[] GetRowAsArray(VectorInt vector, int row) =>
			ShapeOpsCore.GetRowAsArray(vector, row);

		public static VectorInt GetRowAsVector(VectorInt vector, int row) =>
			ShapeOpsCore.GetRowAsVector(vector, row);

		public static VectorInt GetColumnAsVectorX(VectorInt vector, int column) =>
			ShapeOpsCore.GetColumnAsVectorX(vector, column);

		public static int[] GetColumnAsArray(VectorInt vector, int column) =>
			ShapeOpsCore.GetColumnAsArray(vector, column);

		public static VectorInt GetSliceAsVector(VectorInt vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsVector(vector, row_col_index, axis);

		public static VectorInt GetSliceAsVectorX(VectorInt vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsVectorX(vector, row_col_index, axis);

		public static int[] GetSliceAsArray(VectorInt vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsArray(vector, row_col_index, axis);

		public static int[] GetSliceAsArrayX(VectorInt vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsArrayX(vector, row_col_index, axis);

		public static string ToStr(VectorInt vector) =>
			FormattingCore.ToStr(vector);
	}
}

public static class VectorStructuralExtensions
{
	extension(float[] arr)
	{
		public string ToStr(byte decimalplaces = 2) =>
			FormattingCore.ToStr(arr, decimalplaces);

		public void Print(byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(float value)
	{
		public void Print(byte decimalplaces = 2) =>
			FormattingCore.Print(value, decimalplaces);
	}

	extension(float[,] arr)
	{
		public void Print(byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(double value)
	{
		public void Print(byte decimalplaces = 2) =>
			FormattingCore.Print(value, decimalplaces);
	}

	extension(double[] arr)
	{
		public void Print(byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(double[,] arr)
	{
		public void Print(byte decimalplaces = 2) =>
			FormattingCore.Print(arr, decimalplaces);
	}

	extension(int value)
	{
		public void Print() =>
			FormattingCore.Print(value);
	}

	extension(int[] arr)
	{
		public void Print() =>
			FormattingCore.Print(arr);
	}

	extension(int[,] arr)
	{
		public void Print() =>
			FormattingCore.Print(arr);
	}

	extension(uint value)
	{
		public void Print() =>
			FormattingCore.Print(value);
	}

	extension(uint[] arr)
	{
		public void Print() =>
			FormattingCore.Print(arr);
	}

	extension(uint[,] arr)
	{
		public void Print() =>
			FormattingCore.Print(arr);
	}

	extension(long value)
	{
		public void Print() =>
			FormattingCore.Print(value);
	}

	extension(long[] arr)
	{
		public void Print() =>
			FormattingCore.Print(arr);
	}

	extension(long[,] arr)
	{
		public void Print() =>
			FormattingCore.Print(arr);
	}

	extension(Vector vector)
	{
		public Vector ZerosIP(int length, int columns = 0)
		{
			FactoriesCore.ZerosInPlace(vector, length, columns);
			return vector;
		}

		public Vector OnesIP(int length, int columns = 0)
		{
			FactoriesCore.OnesInPlace(vector, length, columns);
			return vector;
		}

		public Vector FillIP(float value, int length, int columns = 0)
		{
			FactoriesCore.FillInPlace(vector, value, length, columns);
			return vector;
		}

		public Vector Append(Vector vectorB) =>
			Vector.Append(vector, vectorB);

		public Vector AppendIP(Vector vectorB)
		{
			ShapeOpsCore.AppendInPlace(vector, vectorB);
			return vector;
		}

		public Vector Prepend(Vector vectorB) =>
			Vector.Prepend(vector, vectorB);

		public Vector Concat(Vector other, ConcatAxis axis = ConcatAxis.Row, bool warp = false) =>
			Vector.Concat(vector, other, axis, warp);

		public Vector ConcatIP(Vector other, ConcatAxis axis = ConcatAxis.Row, bool warp = false) =>
			ShapeOpsCore.ConcatInPlace(vector, other, axis, warp);

		public Vector ConcatColumnX(Vector other, bool warp = false) =>
			Vector.ConcatColumnX(vector, other, warp);

		public Vector ConcatColumnXIP(Vector other, bool warp = false) =>
			ShapeOpsCore.ConcatColumnXInPlace(vector, other, warp);

		public Vector Merge(Vector vectorB) =>
			Vector.Merge(vector, vectorB);

		public Vector MergeIP(Vector vectorB)
		{
			ShapeOpsCore.MergeInPlace(vector, vectorB);
			return vector;
		}

		public Vector Reverse() =>
			Vector.Reverse(vector);

		public Vector ReverseIP()
		{
			ShapeOpsCore.ReverseInPlace(vector);
			return vector;
		}

		public Vector ReverseX() =>
			Vector.ReverseX(vector);

		public Vector ReverseXIP() =>
			ShapeOpsCore.ReverseXInPlace(vector);

		public Vector TransposeX() =>
			Vector.TransposeX(vector);

		public Vector TransposeXIP() =>
			Vector.TransposeXIP(vector);

		public Vector TransferBuffer(Vector temp, bool incColumns = false) =>
			Vector.TransferBuffer(vector, temp, incColumns);

		public float[] GetRowAsArray(int row) =>
			Vector.GetRowAsArray(vector, row);

		public Vector GetRowAsVector(int row) =>
			Vector.GetRowAsVector(vector, row);

		public Vector GetColumnAsVectorX(int column) =>
			Vector.GetColumnAsVectorX(vector, column);

		public float[] GetColumnAsArray(int column) =>
			Vector.GetColumnAsArray(vector, column);

		public Vector GetSliceAsVector(int row_col_index, Axis axis) =>
			Vector.GetSliceAsVector(vector, row_col_index, axis);

		public Vector GetSliceAsVectorX(int row_col_index, Axis axis) =>
			Vector.GetSliceAsVectorX(vector, row_col_index, axis);

		public float[] GetSliceAsArray(int row_col_index, Axis axis) =>
			Vector.GetSliceAsArray(vector, row_col_index, axis);

		public float[] GetSliceAsArrayX(int row_col_index, Axis axis) =>
			Vector.GetSliceAsArrayX(vector, row_col_index, axis);

		public string ToStr(byte decimalplaces = 2) =>
			Vector.ToStr(vector, decimalplaces);
	}

	extension(VectorInt vector)
	{
		public VectorInt ZerosIP(int length, int columns = 0)
		{
			FactoriesCore.ZerosInPlace(vector, length, columns);
			return vector;
		}

		public VectorInt OnesIP(int length, int columns = 0)
		{
			FactoriesCore.OnesInPlace(vector, length, columns);
			return vector;
		}

		public VectorInt FillIP(int value, int length, int columns = 0)
		{
			FactoriesCore.FillInPlace(vector, value, length, columns);
			return vector;
		}

		public VectorInt Append(VectorInt vectorB) =>
			VectorInt.Append(vector, vectorB);

		public VectorInt AppendIP(VectorInt vectorB)
		{
			ShapeOpsCore.AppendInPlace(vector, vectorB);
			return vector;
		}

		public VectorInt Prepend(VectorInt vectorB) =>
			VectorInt.Prepend(vector, vectorB);

		public VectorInt Concat(VectorInt other, ConcatAxis axis = ConcatAxis.Row, bool warp = false) =>
			VectorInt.Concat(vector, other, axis, warp);

		public VectorInt ConcatIP(VectorInt other, ConcatAxis axis = ConcatAxis.Row, bool warp = false) =>
			ShapeOpsCore.ConcatInPlace(vector, other, axis, warp);

		public VectorInt ConcatColumnX(VectorInt other, bool warp = false) =>
			VectorInt.ConcatColumnX(vector, other, warp);

		public VectorInt ConcatColumnXIP(VectorInt other, bool warp = false) =>
			ShapeOpsCore.ConcatColumnXInPlace(vector, other, warp);

		public VectorInt Merge(VectorInt vectorB) =>
			VectorInt.Merge(vector, vectorB);

		public VectorInt MergeIP(VectorInt vectorB)
		{
			ShapeOpsCore.MergeInPlace(vector, vectorB);
			return vector;
		}

		public VectorInt Reverse() =>
			VectorInt.Reverse(vector);

		public VectorInt ReverseIP()
		{
			ShapeOpsCore.ReverseInPlace(vector);
			return vector;
		}

		public VectorInt ReverseX() =>
			VectorInt.ReverseX(vector);

		public VectorInt ReverseXIP() =>
			ShapeOpsCore.ReverseXInPlace(vector);

		public VectorInt TransposeX() =>
			VectorInt.TransposeX(vector);

		public VectorInt TransposeXIP() =>
			VectorInt.TransposeXIP(vector);

		public VectorInt TransferBuffer(VectorInt temp, bool incColumns = false) =>
			VectorInt.TransferBuffer(vector, temp, incColumns);

		public int[] GetRowAsArray(int row) =>
			VectorInt.GetRowAsArray(vector, row);

		public VectorInt GetRowAsVector(int row) =>
			VectorInt.GetRowAsVector(vector, row);

		public VectorInt GetColumnAsVectorX(int column) =>
			VectorInt.GetColumnAsVectorX(vector, column);

		public int[] GetColumnAsArray(int column) =>
			VectorInt.GetColumnAsArray(vector, column);

		public VectorInt GetSliceAsVector(int row_col_index, Axis axis) =>
			VectorInt.GetSliceAsVector(vector, row_col_index, axis);

		public VectorInt GetSliceAsVectorX(int row_col_index, Axis axis) =>
			VectorInt.GetSliceAsVectorX(vector, row_col_index, axis);

		public int[] GetSliceAsArray(int row_col_index, Axis axis) =>
			VectorInt.GetSliceAsArray(vector, row_col_index, axis);

		public int[] GetSliceAsArrayX(int row_col_index, Axis axis) =>
			VectorInt.GetSliceAsArrayX(vector, row_col_index, axis);

		public string ToStr() =>
			VectorInt.ToStr(vector);
	}

	extension(Mask mask){
        public string ToStr() => FormattingCore.ToStr(mask);
        public void Print() => FormattingCore.Print(mask);
    }
}
