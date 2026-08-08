using System.Collections.Generic;
using BAVCL.Geometric;

namespace BAVCL.Modules.Geometric;

public static class Vector3Geometric
{
	extension(Vector3)
	{
		public static Vector MagnitudeX(Vector3 left, Vector3 right) =>
			Vector3Geometry.MagnitudeX(left, right);

		public static Vector MagnitudeX(Vector3 vector) =>
			Vector3Geometry.MagnitudeX(vector);

		public static Vector DistanceX(Vector3 left, Vector3 right) =>
			Vector3Geometry.DistanceX(left, right);

		public static Vector DotX(Vector3 left, Vector3 right) =>
			Vector3Geometry.DotX(left, right);

		public static Vector3 CrossX(Vector3 left, Vector3 right) =>
			Vector3Geometry.CrossX(left, right);

		public static Vector3 NormaliseX(Vector3 vector) =>
			Vector3Geometry.NormaliseX(vector);

		public static Vector3 AccessRow(Vector3 vector, int vertRow) =>
			Vector3Geometry.AccessRow(vector, vertRow);

		public static Vector3 Concat(Vector3 left, Vertex vertA) =>
			Vector3Geometry.Concat(left, vertA);

		public static Vector3 Concat(Vector3 left, Vertex[] vertices) =>
			Vector3Geometry.Concat(left, vertices);

		public static Vector3 Concat(Vector3 left, List<Vertex> vertices) =>
			Vector3Geometry.Concat(left, vertices);

		public static Vector3 Concat(Vector3 left, Vector3 right) =>
			Vector3Geometry.Concat(left, right);

		public static Vector3 Concat(Vector3 left, Vector3[] vectors) =>
			Vector3Geometry.Concat(left, vectors);

		public static Vector3 Concat(Vector3 left, List<Vector3> vectors) =>
			Vector3Geometry.Concat(left, vectors);

		public static string Format(Vector3 vector3, byte decimalplaces = 2) =>
			Vector3Geometry.Format(vector3, decimalplaces);
	}
}

public static class GeometricModule
{
	extension(Vector3 vector)
	{
		public Vector MagnitudeX(Vector3 vectorB) =>
			Vector3.MagnitudeX(vector, vectorB);

		public Vector MagnitudeX() =>
			Vector3.MagnitudeX(vector);

		public Vector DistanceX(Vector3 vectorB) =>
			Vector3.DistanceX(vector, vectorB);

		public Vector DotX(Vector3 vectorB) =>
			Vector3.DotX(vector, vectorB);

		public Vector3 CrossX(Vector3 vectorB) =>
			Vector3.CrossX(vector, vectorB);

		public Vector3 NormaliseX() =>
			Vector3.NormaliseX(vector);

		public Vector3 AccessRow(int vertRow) =>
			Vector3.AccessRow(vector, vertRow);

		public Vector3 Concat(Vertex vertA) =>
			Vector3.Concat(vector, vertA);

		public Vector3 Concat(Vertex[] vertices) =>
			Vector3.Concat(vector, vertices);

		public Vector3 Concat(List<Vertex> vertices) =>
			Vector3.Concat(vector, vertices);

		public Vector3 Concat(Vector3 other) =>
			Vector3.Concat(vector, other);

		public Vector3 Concat(Vector3[] vectors) =>
			Vector3.Concat(vector, vectors);

		public Vector3 Concat(List<Vector3> vectors) =>
			Vector3.Concat(vector, vectors);

		public Vector3 ConcatIP(Vertex vertA) =>
			Vector3Geometry.ConcatIP(vector, vertA);

		public Vector3 ConcatIP(Vertex[] vertices) =>
			Vector3Geometry.ConcatIP(vector, vertices);

		public Vector3 ConcatIP(List<Vertex> vertices) =>
			Vector3Geometry.ConcatIP(vector, vertices);

		public Vector3 ConcatIP(Vector3 other) =>
			Vector3Geometry.ConcatIP(vector, other);

		public Vector3 ConcatIP(Vector3[] vectors) =>
			Vector3Geometry.ConcatIP(vector, vectors);

		public Vector3 ConcatIP(List<Vector3> vectors) =>
			Vector3Geometry.ConcatIP(vector, vectors);

		public string Format(byte decimalplaces = 2) =>
			Vector3.Format(vector, decimalplaces);
	}
}
