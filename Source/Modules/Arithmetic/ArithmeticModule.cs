using BAVCL.Modules.Structural;
using BAVCL.GpuAlgorithms;

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

		public static float SumX(Vector vector) =>
			GlobalReduceAlgorithms.Sum(vector);

		public static Vector Abs(Vector vector) => ElementWiseCore.Abs(vector);

		public static Vector AbsX(Vector vector) => ElementWiseCore.AbsX(vector);

		public static Vector CrossX(Vector left, Vector right) => CrossCore.CrossX(left, right);

		public static float Dot(Vector left, Vector right) => DotProductCore.Dot(left, right);

		public static float Dot(Vector vector, float scalar) => DotProductCore.Dot(vector, scalar);

		public static float DotX(Vector left, Vector right) =>
			GpuDotCore.DotX(left, right);

		public static float DotX(Vector vector, float scalar) =>
			GpuDotCore.DotX(vector, scalar);

		public static Vector DiffX(Vector vector) => ElementWiseCore.DiffX(vector);

		public static Vector ReciprocalX(Vector vector) => ElementWiseCore.ReciprocalX(vector);

		public static Vector Rsqrt(Vector vector) => ElementWiseCore.Rsqrt(vector);

		public static Vector RsqrtX(Vector vector) => ElementWiseCore.RsqrtX(vector);

		public static Vector NanToNumX(Vector vector, float num) => ElementWiseCore.NanToNumX(vector, num);

		public static Vector NormaliseX(Vector vector) => ElementWiseCore.NormaliseX(vector);

		public static Vector MatrixAddX(Vector left, Vector right) => MatrixOpsCore.MatrixAddX(left, right);

		public static Vector MatrixSubtractX(Vector left, Vector right) => MatrixOpsCore.MatrixSubtractX(left, right);

		public static Vector MatrixDivideX(Vector left, Vector right) => MatrixOpsCore.MatrixDivideX(left, right);

		public static Vector MatrixPowX(Vector left, Vector right) => MatrixOpsCore.MatrixPowX(left, right);

		public static Vector MatrixMultiplyX(Vector left, Vector right) => MatrixOpsCore.MatrixMultiplyX(left, right);
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

	extension(VectorInt)
	{
		public static float Sum(VectorInt vector) => SumCore.Sum(vector);

		public static float SumX(VectorInt vector) => GlobalReduceAlgorithms.Sum(vector);

		public static VectorInt Abs(VectorInt vector) => ElementWiseCore.Abs(vector);

		public static VectorInt AbsX(VectorInt vector) => ElementWiseCore.AbsX(vector);

		public static VectorInt CrossX(VectorInt left, VectorInt right) => CrossCore.CrossX(left, right);

		public static float Dot(VectorInt left, VectorInt right) => DotProductCore.Dot(left, right);

		public static float Dot(VectorInt vector, int scalar) => DotProductCore.Dot(vector, scalar);

		public static float DotX(VectorInt left, VectorInt right) => GpuDotCore.DotX(left, right);

		public static float DotX(VectorInt vector, int scalar) => GpuDotCore.DotX(vector, scalar);

		public static VectorInt DiffX(VectorInt vector) => ElementWiseCore.DiffX(vector);

		public static VectorInt MatrixAddX(VectorInt left, VectorInt right) => MatrixOpsCore.MatrixAddX(left, right);

		public static VectorInt MatrixSubtractX(VectorInt left, VectorInt right) => MatrixOpsCore.MatrixSubtractX(left, right);

		public static VectorInt MatrixDivideX(VectorInt left, VectorInt right) => MatrixOpsCore.MatrixDivideX(left, right);

		public static VectorInt MatrixMultiplyX(VectorInt left, VectorInt right) => MatrixOpsCore.MatrixMultiplyX(left, right);
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

		public float SumX() => Vector.SumX(vector);

		public Vector Abs() => Vector.Abs(vector);

		public Vector AbsIP()
		{
			ElementWiseCore.AbsInPlace(vector);
			return vector;
		}

		public Vector AbsX() => Vector.AbsX(vector);

		public Vector AbsXIP()
		{
			ElementWiseCore.AbsXInPlace(vector);
			return vector;
		}

		public Vector CrossX(Vector vectorB) => Vector.CrossX(vector, vectorB);

		public float Dot(Vector vectorB) => Vector.Dot(vector, vectorB);

		public float Dot(float scalar) => Vector.Dot(vector, scalar);

		public float DotX(Vector vectorB) =>
			Vector.DotX(vector, vectorB);

		public float DotX(float scalar) =>
			Vector.DotX(vector, scalar);

		public Vector DiffX() => Vector.DiffX(vector);

		public Vector DiffXIP() => vector.TransferBuffer(Vector.DiffX(vector));

		public Vector ReciprocalX() => Vector.ReciprocalX(vector);

		public Vector ReciprocalXIP()
		{
			ElementWiseCore.ReciprocalXInPlace(vector);
			return vector;
		}

		public Vector Rsqrt() => Vector.Rsqrt(vector);

		public Vector RsqrtIP()
		{
			ElementWiseCore.RsqrtInPlace(vector);
			return vector;
		}

		public Vector RsqrtX() => Vector.RsqrtX(vector);

		public Vector RsqrtXIP()
		{
			ElementWiseCore.RsqrtXInPlace(vector);
			return vector;
		}

		public Vector NanToNumX(float num) => Vector.NanToNumX(vector, num);

		public Vector NanToNumXIP(float num)
		{
			ElementWiseCore.NanToNumXInPlace(vector, num);
			return vector;
		}

		public Vector NormaliseX() => Vector.NormaliseX(vector);

		public Vector NormaliseXIP()
		{
			ElementWiseCore.NormaliseXInPlace(vector);
			return vector;
		}

		public Vector MatrixAddX(Vector matrixB) => Vector.MatrixAddX(vector, matrixB);

		public Vector MatrixSubtractX(Vector matrixB) => Vector.MatrixSubtractX(vector, matrixB);

		public Vector MatrixDivideX(Vector matrixB) => Vector.MatrixDivideX(vector, matrixB);

		public Vector MatrixPowX(Vector matrixB) => Vector.MatrixPowX(vector, matrixB);

		public Vector MatrixMultiplyX(Vector matrixB) => Vector.MatrixMultiplyX(vector, matrixB);
	}

	extension(VectorInt vector)
	{
		public float Sum() => VectorInt.Sum(vector);

		public float SumX() => VectorInt.SumX(vector);

		public VectorInt Abs() => VectorInt.Abs(vector);

		public VectorInt AbsIP()
		{
			ElementWiseCore.AbsInPlace(vector);
			return vector;
		}

		public VectorInt AbsX() => VectorInt.AbsX(vector);

		public VectorInt AbsXIP()
		{
			ElementWiseCore.AbsXInPlace(vector);
			return vector;
		}

		public VectorInt CrossX(VectorInt vectorB) => VectorInt.CrossX(vector, vectorB);

		public float Dot(VectorInt vectorB) => VectorInt.Dot(vector, vectorB);

		public float Dot(int scalar) => VectorInt.Dot(vector, scalar);

		public float DotX(VectorInt vectorB) => VectorInt.DotX(vector, vectorB);

		public float DotX(int scalar) => VectorInt.DotX(vector, scalar);

		public VectorInt DiffX() => VectorInt.DiffX(vector);

		public VectorInt DiffXIP() => vector.TransferBuffer(VectorInt.DiffX(vector));

		public VectorInt MatrixAddX(VectorInt matrixB) => VectorInt.MatrixAddX(vector, matrixB);

		public VectorInt MatrixSubtractX(VectorInt matrixB) => VectorInt.MatrixSubtractX(vector, matrixB);

		public VectorInt MatrixDivideX(VectorInt matrixB) => VectorInt.MatrixDivideX(vector, matrixB);

		public VectorInt MatrixMultiplyX(VectorInt matrixB) => VectorInt.MatrixMultiplyX(vector, matrixB);
	}
}
