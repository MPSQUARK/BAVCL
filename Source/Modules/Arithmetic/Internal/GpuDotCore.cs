using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Arithmetic;

internal static class GpuDotCore
{
	internal static float DotX(Vector left, Vector right) =>
		GlobalReduceAlgorithms.Dot(left, right);

	internal static float DotX(Vector vector, float scalar) =>
		GlobalReduceAlgorithms.Dot(vector, scalar);

	internal static float DotX(VectorInt left, VectorInt right) =>
		GlobalReduceAlgorithms.Dot(left, right);

	internal static float DotX(VectorInt vector, int scalar) =>
		GlobalReduceAlgorithms.Dot(vector, scalar);
}
