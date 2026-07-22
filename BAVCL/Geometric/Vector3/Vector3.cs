using System;
using BAVCL.Core;
using BAVCL.Modules.Geometric;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
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

	public override string ToString() => this.Format(2);
}
