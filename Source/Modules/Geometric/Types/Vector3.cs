using System;
using BAVCL.Geometric.Enums;
using BAVCL.Modules.Geometric;
using BAVCL.Modules.GpuOps;

namespace BAVCL.Geometric;

public sealed class Vector3 : VectorBase<float>
{
	public override int Columns { get { return _columns; } set { _columns = 3; } }

	public Vector3(GPU gpu, float[] value, bool cache = true) : base(gpu, ValidateVectorLength(value), 3, cache) { }

	public Vector3(GPU gpu, int length) : base(gpu, ValidateVectorLength(length), 3) { }

	static float[] ValidateVectorLength(float[] values)
	{
		if (values.Length % 3 != 0) throw new Exception($"Vector3 must have a length that is a multiple of 3. Recieved {values.Length}");
		return values;
	}

	static int ValidateVectorLength(int length)
	{
		if (length % 3 != 0) throw new Exception($"Vector3 must have a length that is a multiple of 3. Recieved {length}");
		return length;
	}

	public Vector ToVector(bool cache = true)
	{
		if (_id != 0)
			return new Vector(Gpu, Pull(), Columns, cache);

		return new Vector(Gpu, ToArray(), Columns, cache);
	}

	public Vector ToVector(int columns, bool cache = true)
	{
		if (_id != 0)
			return new Vector(Gpu, Pull(), Columns, cache);

		return new Vector(Gpu, ToArray(), columns, cache);
	}

	public Vector3 Copy()
	{
		if (_id != 0)
			return new Vector3(Gpu, Pull());

		return new Vector3(Gpu, ToArray());
	}

	public float this[int i, Coord coord]
	{
		get => GetAt(i, coord);
		set => SetAt(i, coord, value);
	}

	public float GetAt(int row, Coord coord)
	{
		if (row < 0 || row > RowCount())
			throw new IndexOutOfRangeException();

		return GetAt(row, (int)coord);
	}

	public void SetAt(int row, Coord coord, float value)
	{
		if (row < 0 || row > RowCount())
			throw new IndexOutOfRangeException();

		SetAt(row, (int)coord, value);
	}

	public override string ToString() => this.Format(2);

	public static Vector3 operator +(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.add);

	public static Vector3 operator -(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.subtract);

	public static Vector3 operator *(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.multiply);

	public static Vector3 operator /(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.divide);

	public static Vector3 operator ^(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OP(vectorB, Operations.pow);

	public static Vector3 operator +(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.add);

	public static Vector3 operator -(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.subtract);

	public static Vector3 operator *(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.multiply);

	public static Vector3 operator /(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.divide);

	public static Vector3 operator ^(Vector3 vector, float scalar) =>
		vector.OP(scalar, Operations.pow);

	public static Vector3 operator +(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.add);

	public static Vector3 operator -(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.flipSubtract);

	public static Vector3 operator *(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.multiply);

	public static Vector3 operator /(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.flipDivide);

	public static Vector3 operator ^(float scalar, Vector3 vector) =>
		vector.OP(scalar, Operations.flipPow);
}
