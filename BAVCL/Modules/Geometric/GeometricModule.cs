using System.Collections.Generic;
using BAVCL.Geometric;

namespace BAVCL.Modules.Geometric;

public static class Vector3Geometric
{
	extension(Vector3)
	{
		public static Vector Magnitude(Vector3 left, Vector3 right) =>
			Vector3Geometry.Magnitude(left, right);

		public static Vector Magnitude(Vector3 vector) =>
			Vector3Geometry.Magnitude(vector);

		public static Vector Distance(Vector3 left, Vector3 right) =>
			Vector3Geometry.Distance(left, right);

		public static Vector3 Cross(Vector3 left, Vector3 right) =>
			Vector3Geometry.Cross(left, right);

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
		public Vector Magnitude(Vector3 vectorB) =>
			Vector3.Magnitude(vector, vectorB);

		public Vector Magnitude() =>
			Vector3.Magnitude(vector);

		public Vector Distance(Vector3 vectorB) =>
			Vector3.Distance(vector, vectorB);

		public Vector3 Cross(Vector3 vectorB) =>
			Vector3.Cross(vector, vectorB);

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

		public Vector3 Concat_IP(Vertex vertA) =>
			Vector3Geometry.Concat_IP(vector, vertA);

		public Vector3 Concat_IP(Vertex[] vertices) =>
			Vector3Geometry.Concat_IP(vector, vertices);

		public Vector3 Concat_IP(List<Vertex> vertices) =>
			Vector3Geometry.Concat_IP(vector, vertices);

		public Vector3 Concat_IP(Vector3 other) =>
			Vector3Geometry.Concat_IP(vector, other);

		public Vector3 Concat_IP(Vector3[] vectors) =>
			Vector3Geometry.Concat_IP(vector, vectors);

		public Vector3 Concat_IP(List<Vector3> vectors) =>
			Vector3Geometry.Concat_IP(vector, vectors);

		public string Format(byte decimalplaces = 2) =>
			Vector3.Format(vector, decimalplaces);
	}
}
