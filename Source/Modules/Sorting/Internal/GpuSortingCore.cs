using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Sorting;

internal static class GpuSortingCore
{
	internal static VectorInt SortX(VectorInt vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			return SortAscX(vector);

		return SortDescX(vector);
	}

	internal static VectorInt SortAscX(VectorInt vector) => SortAlgorithms.SortAscX(vector);

	internal static VectorInt SortDescX(VectorInt vector) => SortAlgorithms.SortDescX(vector);

	internal static Vector SortX(Vector vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			return SortAscX(vector);

		return SortDescX(vector);
	}

	internal static Vector SortAscX(Vector vector) => SortAlgorithms.SortAscX(vector);

	internal static Vector SortDescX(Vector vector) => SortAlgorithms.SortDescX(vector);

	internal static void SortXIP(VectorInt vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			SortAscXIP(vector);
		else
			SortDescXIP(vector);
	}

	internal static void SortAscXIP(VectorInt vector) => SortAlgorithms.SortAscXIP(vector);

	internal static void SortDescXIP(VectorInt vector) => SortAlgorithms.SortDescXIP(vector);

	internal static void SortXIP(Vector vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			SortAscXIP(vector);
		else
			SortDescXIP(vector);
	}

	internal static void SortAscXIP(Vector vector) => SortAlgorithms.SortAscXIP(vector);

	internal static void SortDescXIP(Vector vector) => SortAlgorithms.SortDescXIP(vector);

	internal static VectorInt ArgsortX(VectorInt vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			return ArgsortAscX(vector);

		return ArgsortDescX(vector);
	}

	internal static VectorInt ArgsortAscX(VectorInt vector) => SortAlgorithms.ArgsortAscX(vector);

	internal static VectorInt ArgsortDescX(VectorInt vector) => SortAlgorithms.ArgsortDescX(vector);

	internal static VectorInt ArgsortX(Vector vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			return ArgsortAscX(vector);

		return ArgsortDescX(vector);
	}

	internal static VectorInt ArgsortAscX(Vector vector) => SortAlgorithms.ArgsortAscX(vector);

	internal static VectorInt ArgsortDescX(Vector vector) => SortAlgorithms.ArgsortDescX(vector);

	internal static void ArgsortXIP(VectorInt vector, VectorInt indices, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			ArgsortAscXIP(vector, indices);
		else
			ArgsortDescXIP(vector, indices);
	}

	internal static void ArgsortAscXIP(VectorInt vector, VectorInt indices) =>
		SortAlgorithms.ArgsortAscXIP(vector, indices);

	internal static void ArgsortDescXIP(VectorInt vector, VectorInt indices) =>
		SortAlgorithms.ArgsortDescXIP(vector, indices);

	internal static void ArgsortXIP(Vector vector, VectorInt indices, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			ArgsortAscXIP(vector, indices);
		else
			ArgsortDescXIP(vector, indices);
	}

	internal static void ArgsortAscXIP(Vector vector, VectorInt indices) =>
		SortAlgorithms.ArgsortAscXIP(vector, indices);

	internal static void ArgsortDescXIP(Vector vector, VectorInt indices) =>
		SortAlgorithms.ArgsortDescXIP(vector, indices);
}
