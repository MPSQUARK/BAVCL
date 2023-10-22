// using System;
// using BAVCL.Core;

// namespace BAVCL.Geometric;

// public sealed partial class Vector3 : VectorBase<float>
// {
// 	public static Vector Magnitude(Vector3 vectorA, Vector3 vectorB)
// 	{
// 		if (vectorA.Length != vectorB.Length) 
// 		{ 
// 			throw new Exception($"Cannot Cross Product two Vector3's together of different lengths. {vectorA.Length} != {vectorB.Length}"); 
// 		}
		
// 		return VOP(vectorA, vectorB, Operations.distance);
// 	}
// }
