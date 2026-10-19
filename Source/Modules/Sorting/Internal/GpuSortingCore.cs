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

	internal static VectorInt SortAscX(VectorInt vector)
	{
		VectorInt output = vector.Copy();
		SortAscXIP(output);
		return output;
	}

	internal static VectorInt SortDescX(VectorInt vector)
	{
		VectorInt output = vector.Copy();
		SortDescXIP(output);
		return output;
	}

	internal static void SortXIP(VectorInt vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			SortAscXIP(vector);
		else
			SortDescXIP(vector);
	}

	internal static void SortAscXIP(VectorInt vector)
	{
		if (vector.Length <= 1)
			return;

		if (vector.Is2D())
			SortAlgorithms.Sort2DAscXIP(vector);
		else
			SortAlgorithms.Sort1DAscXIP(vector);
	}

	internal static void SortDescXIP(VectorInt vector)
	{
		if (vector.Length <= 1)
			return;

		if (vector.Is2D())
			SortAlgorithms.Sort2DDescXIP(vector);
		else
			SortAlgorithms.Sort1DDescXIP(vector);
	}

	internal static Vector SortX(Vector vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			return SortAscX(vector);

		return SortDescX(vector);
	}

	internal static Vector SortAscX(Vector vector)
	{
		Vector output = vector.Copy();
		SortAscXIP(output);
		return output;
	}

	internal static Vector SortDescX(Vector vector)
	{
		Vector output = vector.Copy();
		SortDescXIP(output);
		return output;
	}

	internal static void SortXIP(Vector vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			SortAscXIP(vector);
		else
			SortDescXIP(vector);
	}

	internal static void SortAscXIP(Vector vector)
	{
		if (vector.Length <= 1)
			return;

		if (vector.Is2D())
			SortAlgorithms.Sort2DAscXIP(vector);
		else
			SortAlgorithms.Sort1DAscXIP(vector);
	}

	internal static void SortDescXIP(Vector vector)
	{
		if (vector.Length <= 1)
			return;

		if (vector.Is2D())
			SortAlgorithms.Sort2DDescXIP(vector);
		else
			SortAlgorithms.Sort1DDescXIP(vector);
	}

	internal static VectorInt ArgsortX(VectorInt vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			return ArgsortAscX(vector);

		return ArgsortDescX(vector);
	}

	internal static VectorInt ArgsortAscX(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		if (vector.Is2D())
			return SortAlgorithms.Argsort2DAscX(vector);

		return SortAlgorithms.Argsort1DAscX(vector);
	}

	internal static VectorInt ArgsortDescX(VectorInt vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		if (vector.Is2D())
			return SortAlgorithms.Argsort2DDescX(vector);

		return SortAlgorithms.Argsort1DDescX(vector);
	}

	internal static void ArgsortXIP(VectorInt vector, VectorInt indices, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			ArgsortAscXIP(vector, indices);
		else
			ArgsortDescXIP(vector, indices);
	}

	internal static void ArgsortAscXIP(VectorInt vector, VectorInt indices)
	{
		Guard.MatchingGpuPairIndices(vector, indices);
		if (vector.Length == 0)
			return;

		if (vector.Is2D())
			SortAlgorithms.Argsort2DAscXIP(vector, indices);
		else
			SortAlgorithms.Argsort1DAscXIP(vector, indices);
	}

	internal static void ArgsortDescXIP(VectorInt vector, VectorInt indices)
	{
		Guard.MatchingGpuPairIndices(vector, indices);
		if (vector.Length == 0)
			return;

		if (vector.Is2D())
			SortAlgorithms.Argsort2DDescXIP(vector, indices);
		else
			SortAlgorithms.Argsort1DDescXIP(vector, indices);
	}

	internal static VectorInt ArgsortX(Vector vector, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			return ArgsortAscX(vector);

		return ArgsortDescX(vector);
	}

	internal static VectorInt ArgsortAscX(Vector vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		if (vector.Is2D())
			return SortAlgorithms.Argsort2DAscX(vector);

		return SortAlgorithms.Argsort1DAscX(vector);
	}

	internal static VectorInt ArgsortDescX(Vector vector)
	{
		if (vector.Length == 0)
			return new VectorInt(vector.Gpu, 0, vector.Columns);

		if (vector.Is2D())
			return SortAlgorithms.Argsort2DDescX(vector);

		return SortAlgorithms.Argsort1DDescX(vector);
	}

	internal static void ArgsortXIP(Vector vector, VectorInt indices, SortOrder order)
	{
		if (order == SortOrder.Ascending)
			ArgsortAscXIP(vector, indices);
		else
			ArgsortDescXIP(vector, indices);
	}

	internal static void ArgsortAscXIP(Vector vector, VectorInt indices)
	{
		Guard.MatchingGpuPairIndices(vector, indices);
		if (vector.Length == 0)
			return;

		if (vector.Is2D())
			SortAlgorithms.Argsort2DAscXIP(vector, indices);
		else
			SortAlgorithms.Argsort1DAscXIP(vector, indices);
	}

	internal static void ArgsortDescXIP(Vector vector, VectorInt indices)
	{
		Guard.MatchingGpuPairIndices(vector, indices);
		if (vector.Length == 0)
			return;

		if (vector.Is2D())
			SortAlgorithms.Argsort2DDescXIP(vector, indices);
		else
			SortAlgorithms.Argsort1DDescXIP(vector, indices);
	}
}
