using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
{
	public static Vector Magnitude(Vector3 vectorA, Vector3 vectorB)
	{
		if (vectorA.Length != vectorB.Length) 
			throw new LengthMismatchException(nameof(Magnitude), vectorA.Length, vectorB.Length);
		
		return VOP(vectorA, vectorB, Operations.magnitude);
	}
	
	public static Vector Magnitude(Vector3 vector)
	{
		return VOP(vector, Operations.magnitude);
	}
	
	public Vector Magnitude() => Magnitude(this);
	
}
