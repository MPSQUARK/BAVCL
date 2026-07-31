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

		public static Vector Concat(Vector left, Vector right, char axis = 'r', bool warp = false) =>
			ShapeOpsCore.Concat(left, right, axis, warp);

		public static Vector Merge(Vector left, Vector right) =>
			ShapeOpsCore.Merge(left, right);

		public static Vector Reverse(Vector vector) =>
			ShapeOpsCore.Reverse(vector);

		public static Vector ReverseX(Vector vector) =>
			ShapeOpsCore.ReverseX(vector);

		public static Vector Transpose(Vector vector) =>
			ShapeOpsCore.Transpose(vector);

		public static Vector Transpose_IP(Vector vector) =>
			ShapeOpsCore.TransposeInPlace(vector);

		public static Vector TransferBuffer(Vector inheritee, Vector temp, bool incColumns = false) =>
			ShapeOpsCore.TransferBuffer(inheritee, temp, incColumns);

		public static float[] GetRowAsArray(Vector vector, int row) =>
			ShapeOpsCore.GetRowAsArray(vector, row);

		public static float[] GetRowAsArray(Vector vector, int row, bool noSync) =>
			ShapeOpsCore.GetRowAsArray(vector, row, noSync);

		public static Vector GetRowAsVector(Vector vector, int row) =>
			ShapeOpsCore.GetRowAsVector(vector, row);

		public static Vector GetColumnAsVector(Vector vector, int column) =>
			ShapeOpsCore.GetColumnAsVector(vector, column);

		public static float[] GetColumnAsArray(Vector vector, int column) =>
			ShapeOpsCore.GetColumnAsArray(vector, column);

		public static Vector GetSliceAsVector(Vector vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsVector(vector, row_col_index, axis);

		public static float[] GetSliceAsArray(Vector vector, int row_col_index, Axis axis) =>
			ShapeOpsCore.GetSliceAsArray(vector, row_col_index, axis);

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
		public Vector Zeros_IP(int length, int columns = 0)
		{
			FactoriesCore.ZerosInPlace(vector, length, columns);
			return vector;
		}

		public Vector Ones_IP(int length, int columns = 0)
		{
			FactoriesCore.OnesInPlace(vector, length, columns);
			return vector;
		}

		public Vector Fill_IP(float value, int length, int columns = 0)
		{
			FactoriesCore.FillInPlace(vector, value, length, columns);
			return vector;
		}

		public Vector Append(Vector vectorB) =>
			Vector.Append(vector, vectorB);

		public Vector Append_IP(Vector vectorB)
		{
			ShapeOpsCore.AppendInPlace(vector, vectorB);
			return vector;
		}

		public Vector Prepend(Vector vectorB) =>
			Vector.Prepend(vector, vectorB);

		public Vector Concat(Vector other, char axis = 'r', bool warp = false) =>
			Vector.Concat(vector, other, axis, warp);

		public Vector Concat_IP(Vector other, char axis = 'r', bool warp = false) =>
			ShapeOpsCore.ConcatInPlace(vector, other, axis, warp);

		public Vector Merge(Vector vectorB) =>
			Vector.Merge(vector, vectorB);

		public Vector Merge_IP(Vector vectorB)
		{
			ShapeOpsCore.MergeInPlace(vector, vectorB);
			return vector;
		}

		public Vector Reverse() =>
			Vector.Reverse(vector);

		public Vector Reverse_IP()
		{
			ShapeOpsCore.ReverseInPlace(vector);
			return vector;
		}

		public Vector ReverseX() =>
			Vector.ReverseX(vector);

		public Vector ReverseX_IP() =>
			ShapeOpsCore.ReverseXInPlace(vector);

		public Vector Transpose() =>
			Vector.Transpose(vector);

		public Vector Transpose_IP() =>
			Vector.Transpose_IP(vector);

		public Vector TransferBuffer(Vector temp, bool incColumns = false) =>
			Vector.TransferBuffer(vector, temp, incColumns);

		public float[] GetRowAsArray(int row) =>
			Vector.GetRowAsArray(vector, row);

		public float[] GetRowAsArray(int row, bool noSync) =>
			Vector.GetRowAsArray(vector, row, noSync);

		public Vector GetRowAsVector(int row) =>
			Vector.GetRowAsVector(vector, row);

		public Vector GetColumnAsVector(int column) =>
			Vector.GetColumnAsVector(vector, column);

		public float[] GetColumnAsArray(int column) =>
			Vector.GetColumnAsArray(vector, column);

		public Vector GetSliceAsVector(int row_col_index, Axis axis) =>
			Vector.GetSliceAsVector(vector, row_col_index, axis);

		public float[] GetSliceAsArray(int row_col_index, Axis axis) =>
			Vector.GetSliceAsArray(vector, row_col_index, axis);

		public string ToStr(byte decimalplaces = 2) =>
			Vector.ToStr(vector, decimalplaces);
	}

	extension(Mask mask){
        public string ToStr() => FormattingCore.ToStr(mask);
        public void Print() => FormattingCore.Print(mask);
    }
}
