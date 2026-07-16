using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
{
	public static Vector Distance(Vector3 vectorA, Vector3 vectorB)
	{
		if (vectorA.Length != vectorB.Length) 
			throw new Vector3LengthMismatchException(nameof(Distance), vectorA.Length, vectorB.Length);
		
		return VOP(vectorA, vectorB, Operations.distance);
	}
	
	public Vector Distance(Vector3 vector) => Distance(this, vector);
}
