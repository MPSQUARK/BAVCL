using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Sorting;

internal static class GpuSortingCore
{
	internal static VectorInt SortX(VectorInt vector, SortOrder order)
	{
		VectorInt copy = vector.Copy();
		SortXIP(copy, order);
		return copy;
	}

	internal static VectorInt SortAscX(VectorInt vector) => SortX(vector, SortOrder.Ascending);

	internal static VectorInt SortDescX(VectorInt vector) => SortX(vector, SortOrder.Descending);

	internal static Vector SortX(Vector vector, SortOrder order)
	{
		Vector copy = vector.Copy();
		SortXIP(copy, order);
		return copy;
	}

	internal static Vector SortAscX(Vector vector) => SortX(vector, SortOrder.Ascending);

	internal static Vector SortDescX(Vector vector) => SortX(vector, SortOrder.Descending);

	internal static void SortXIP(VectorInt vector, SortOrder order) => SortAlgorithms.SortIntXIP(vector, order);

	internal static void SortAscXIP(VectorInt vector) => SortXIP(vector, SortOrder.Ascending);

	internal static void SortDescXIP(VectorInt vector) => SortXIP(vector, SortOrder.Descending);

	internal static void SortXIP(Vector vector, SortOrder order) => SortAlgorithms.SortFloatXIP(vector, order);

	internal static void SortAscXIP(Vector vector) => SortXIP(vector, SortOrder.Ascending);

	internal static void SortDescXIP(Vector vector) => SortXIP(vector, SortOrder.Descending);

	internal static VectorInt ArgsortX(VectorInt vector, SortOrder order) => SortAlgorithms.ArgsortIntX(vector, order);

	internal static VectorInt ArgsortAscX(VectorInt vector) => ArgsortX(vector, SortOrder.Ascending);

	internal static VectorInt ArgsortDescX(VectorInt vector) => ArgsortX(vector, SortOrder.Descending);

	internal static VectorInt ArgsortX(Vector vector, SortOrder order) => SortAlgorithms.ArgsortFloatX(vector, order);

	internal static VectorInt ArgsortAscX(Vector vector) => ArgsortX(vector, SortOrder.Ascending);

	internal static VectorInt ArgsortDescX(Vector vector) => ArgsortX(vector, SortOrder.Descending);

	internal static void ArgsortXIP(VectorInt vector, VectorInt indices, SortOrder order) =>
		SortAlgorithms.ArgsortIntXIP(vector, indices, order);

	internal static void ArgsortAscXIP(VectorInt vector, VectorInt indices) => ArgsortXIP(vector, indices, SortOrder.Ascending);

	internal static void ArgsortDescXIP(VectorInt vector, VectorInt indices) => ArgsortXIP(vector, indices, SortOrder.Descending);

	internal static void ArgsortXIP(Vector vector, VectorInt indices, SortOrder order) =>
		SortAlgorithms.ArgsortFloatXIP(vector, indices, order);

	internal static void ArgsortAscXIP(Vector vector, VectorInt indices) => ArgsortXIP(vector, indices, SortOrder.Ascending);

	internal static void ArgsortDescXIP(Vector vector, VectorInt indices) => ArgsortXIP(vector, indices, SortOrder.Descending);
}
