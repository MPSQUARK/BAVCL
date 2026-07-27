using BAVCL.Modules.Structural;

namespace BAVCL.Modules.Arithmetic;

public static class ArithmeticModule
{
	extension(float[])
	{
		public static float Sum(float[] array) => SumCore.Sum(array);
	}

	extension(double[])
	{
		public static double Sum(double[] array) => SumCore.Sum(array);
	}

	extension(Vector)
	{
		public static float Sum(Vector vector) => SumCore.Sum(vector);

		public static Vector Abs(Vector vector) => ElementWiseCore.Abs(vector);

		public static Vector AbsX(Vector vector) => ElementWiseCore.AbsX(vector);

		public static Vector Cross(Vector left, Vector right) => CrossCore.Cross(left, right);

		public static float Dot(Vector left, Vector right) => DotProductCore.Dot(left, right);

		public static float Dot(Vector vector, float scalar) => DotProductCore.Dot(vector, scalar);

		public static Vector Diff(Vector vector) => ElementWiseCore.Diff(vector);

		public static Vector Reciprocal(Vector vector) => ElementWiseCore.Reciprocal(vector);

		public static Vector Rsqrt(Vector vector) => ElementWiseCore.Rsqrt(vector);

		public static Vector RsqrtX(Vector vector) => ElementWiseCore.RsqrtX(vector);

		public static Vector Nan_to_num(Vector vector, float num) => ElementWiseCore.Nan_to_num(vector, num);

		public static Vector Normalise(Vector vector) => ElementWiseCore.Normalise(vector);

		public static Vector MatrixAdd(Vector left, Vector right) => MatrixOpsCore.MatrixAdd(left, right);

		public static Vector MatrixSubtract(Vector left, Vector right) => MatrixOpsCore.MatrixSubtract(left, right);

		public static Vector MatrixDivide(Vector left, Vector right) => MatrixOpsCore.MatrixDivide(left, right);

		public static Vector MatrixPow(Vector left, Vector right) => MatrixOpsCore.MatrixPow(left, right);

		public static Vector MatrixMultiply(Vector left, Vector right) => MatrixOpsCore.MatrixMultiply(left, right);
	}

	extension(int[])
	{
		public static float Sum(int[] array) => SumCore.Sum(array);
	}

	extension(long[])
	{
		public static float Sum(long[] array) => SumCore.Sum(array);
	}

	extension(BAVCL.Geometric.Vector3)
	{
		public static float Sum(BAVCL.Geometric.Vector3 vector3) => SumCore.Sum(vector3);
	}
}

public static class VectorArithmeticExtensions
{
	extension(float[] array)
	{
		public float Sum() => SumCore.Sum(array);
	}

	extension(double[] array)
	{
		public double Sum() => SumCore.Sum(array);
	}

	extension(int[] array)
	{
		public float Sum() => SumCore.Sum(array);
	}

	extension(long[] array)
	{
		public float Sum() => SumCore.Sum(array);
	}

	extension(BAVCL.Geometric.Vector3 vector3)
	{
		public float Sum() => SumCore.Sum(vector3);
	}

	extension(Vector vector)
	{
		public float Sum() => Vector.Sum(vector);

		public Vector Abs() => Vector.Abs(vector);

		public Vector Abs_IP()
		{
			ElementWiseCore.AbsInPlace(vector);
			return vector;
		}

		public Vector AbsX() => Vector.AbsX(vector);

		public Vector AbsX_IP()
		{
			ElementWiseCore.AbsXInPlace(vector);
			return vector;
		}

		public Vector Cross(Vector vectorB) => Vector.Cross(vector, vectorB);

		public float Dot(Vector vectorB) => Vector.Dot(vector, vectorB);

		public float Dot(float scalar) => Vector.Dot(vector, scalar);

		public Vector Diff() => Vector.Diff(vector);

		public Vector Diff_IP() => vector.TransferBuffer(Vector.Diff(vector));

		public Vector Reciprocal() => Vector.Reciprocal(vector);

		public Vector Reciprocal_IP()
		{
			ElementWiseCore.ReciprocalInPlace(vector);
			return vector;
		}

		public Vector Rsqrt() => Vector.Rsqrt(vector);

		public Vector Rsqrt_IP()
		{
			ElementWiseCore.RsqrtInPlace(vector);
			return vector;
		}

		public Vector RsqrtX() => Vector.RsqrtX(vector);

		public Vector RsqrtX_IP()
		{
			ElementWiseCore.RsqrtXInPlace(vector);
			return vector;
		}

		public Vector Nan_to_num(float num) => Vector.Nan_to_num(vector, num);

		public Vector Nan_to_num_IP(float num)
		{
			ElementWiseCore.Nan_to_numInPlace(vector, num);
			return vector;
		}

		public Vector Normalise() => Vector.Normalise(vector);

		public Vector Normalise_IP()
		{
			ElementWiseCore.NormaliseInPlace(vector);
			return vector;
		}

		public Vector MatrixAdd(Vector matrixB) => Vector.MatrixAdd(vector, matrixB);

		public Vector MatrixSubtract(Vector matrixB) => Vector.MatrixSubtract(vector, matrixB);

		public Vector MatrixDivide(Vector matrixB) => Vector.MatrixDivide(vector, matrixB);

		public Vector MatrixPow(Vector matrixB) => Vector.MatrixPow(vector, matrixB);

		public Vector MatrixMultiply(Vector matrixB) => Vector.MatrixMultiply(vector, matrixB);
	}
}
