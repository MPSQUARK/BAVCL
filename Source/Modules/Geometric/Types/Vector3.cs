using System;
using BAVCL.Geometric.Enums;
using BAVCL.Modules.Geometric;
using BAVCL.Modules.GpuOps;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Geometric;

public sealed class Vector3 : VectorBase<float>
{
	public override int Columns { get { return _columns; } set { _columns = 3; } }

	public Vector3(GPU? gpu = null) : base(gpu) { _columns = 3; }

	public Vector3(float[] values, Upload upload = Upload.Now, GPU? gpu = null)
		: base(ValidateVectorLength(values), 3, upload, gpu) { }

	public Vector3(int length, Init init = Init.UnInit, GPU? gpu = null)
		: base(ValidateVectorLength(length), 3, init, gpu) { }

	public override void Assign(float[] values, int columns = 3, Upload upload = Upload.Now)
	{
		ValidateVectorLength(values);
		base.Assign(values, 3, upload);
	}

	public void Assign(float[] values, Upload upload) => Assign(values, 3, upload);

	public override void Assign(int length, int columns = 3, Init init = Init.UnInit)
	{
		ValidateVectorLength(length);
		base.Assign(length, 3, init);
	}

	public void Assign(int length, Init init) => Assign(length, 3, init);

	public override void Assign(ArrayView1D<float, Stride1D.Dense> deviceView, int columns = 3)
	{
		ValidateVectorLength(deviceView.IntLength);
		base.Assign(deviceView, 3);
	}

	static float[] ValidateVectorLength(float[] values)
	{
		ArgumentNullException.ThrowIfNull(values);
		if (values.Length % 3 != 0) throw new Exception($"Vector3 must have a length that is a multiple of 3. Recieved {values.Length}");
		return values;
	}

	static int ValidateVectorLength(int length)
	{
		if (length % 3 != 0) throw new Exception($"Vector3 must have a length that is a multiple of 3. Recieved {length}");
		return length;
	}

	public Vector ToVector(Upload upload = Upload.Now)
	{
		if (_id != 0)
			return new Vector(Pull(), Columns, upload, Gpu);

		return new Vector(ToArray(), Columns, upload, Gpu);
	}

	public Vector ToVector(int columns, Upload upload = Upload.Now)
	{
		if (_id != 0)
			return new Vector(Pull(), Columns, upload, Gpu);

		return new Vector(ToArray(), columns, upload, Gpu);
	}

	public Vector3 Copy(Upload upload = Upload.Now)
	{
		if (_id != 0)
			return new Vector3(Pull(), upload, Gpu);

		return new Vector3(ToArray(), upload, Gpu);
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
		vectorA.OPX(vectorB, Operations.add);

	public static Vector3 operator -(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OPX(vectorB, Operations.subtract);

	public static Vector3 operator *(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OPX(vectorB, Operations.multiply);

	public static Vector3 operator /(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OPX(vectorB, Operations.divide);

	public static Vector3 operator ^(Vector3 vectorA, Vector3 vectorB) =>
		vectorA.OPX(vectorB, Operations.pow);

	public static Vector3 operator +(Vector3 vector, float scalar) =>
		vector.OPX(scalar, Operations.add);

	public static Vector3 operator -(Vector3 vector, float scalar) =>
		vector.OPX(scalar, Operations.subtract);

	public static Vector3 operator *(Vector3 vector, float scalar) =>
		vector.OPX(scalar, Operations.multiply);

	public static Vector3 operator /(Vector3 vector, float scalar) =>
		vector.OPX(scalar, Operations.divide);

	public static Vector3 operator ^(Vector3 vector, float scalar) =>
		vector.OPX(scalar, Operations.pow);

	public static Vector3 operator +(float scalar, Vector3 vector) =>
		vector.OPX(scalar, Operations.add);

	public static Vector3 operator -(float scalar, Vector3 vector) =>
		vector.OPX(scalar, Operations.flipSubtract);

	public static Vector3 operator *(float scalar, Vector3 vector) =>
		vector.OPX(scalar, Operations.multiply);

	public static Vector3 operator /(float scalar, Vector3 vector) =>
		vector.OPX(scalar, Operations.flipDivide);

	public static Vector3 operator ^(float scalar, Vector3 vector) =>
		vector.OPX(scalar, Operations.flipPow);
}
