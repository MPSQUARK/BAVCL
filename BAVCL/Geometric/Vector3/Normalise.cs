using System;
using BAVCL.Core;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
{
	public static Vector Normalise(Vector3 vector)
	{
		return vector.ToVector() / VOP(vector, vector, Operations.magnitude);
	}
	
	public Vector Normalise() => this.ToVector() / Magnitude(this);
	
}
