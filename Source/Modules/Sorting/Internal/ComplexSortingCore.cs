using System;
using BAVCL.GpuAlgorithms;

namespace BAVCL.Modules.Sorting;

internal static class ComplexSortingCore
{
	internal static VectorComplex64 SortAscX(VectorComplex64 vector, ComplexSortMode mode)
	{
		Validate(mode);
		VectorComplex64 output = vector.Copy();
		SortAscXIP(output, mode);
		return output;
	}

	internal static VectorComplex64 SortDescX(VectorComplex64 vector, ComplexSortMode mode)
	{
		Validate(mode);
		VectorComplex64 output = vector.Copy();
		SortDescXIP(output, mode);
		return output;
	}

	internal static void SortAscXIP(VectorComplex64 vector, ComplexSortMode mode)
	{
		Validate(mode);
		if (vector.Length <= 1)
			return;
		if (vector.Is2D())
			SortAlgorithms.Sort2DAscXIP(vector, mode);
		else
			SortAlgorithms.Sort1DAscXIP(vector, mode);
	}

	internal static void SortDescXIP(VectorComplex64 vector, ComplexSortMode mode)
	{
		Validate(mode);
		if (vector.Length <= 1)
			return;
		if (vector.Is2D())
			SortAlgorithms.Sort2DDescXIP(vector, mode);
		else
			SortAlgorithms.Sort1DDescXIP(vector, mode);
	}

	internal static VectorInt ArgsortAscX(VectorComplex64 vector, ComplexSortMode mode)
	{
		Validate(mode);
		if (vector.Length == 0)
			return new VectorInt(0, vector.Columns, gpu: vector.Gpu);
		if (vector.Length == 1)
			return new VectorInt([0], vector.Columns, gpu: vector.Gpu);
		if (vector.Is2D())
			return SortAlgorithms.Argsort2DAscX(vector, mode);
		return SortAlgorithms.Argsort1DAscX(vector, mode);
	}

	internal static VectorInt ArgsortDescX(VectorComplex64 vector, ComplexSortMode mode)
	{
		Validate(mode);
		if (vector.Length == 0)
			return new VectorInt(0, vector.Columns, gpu: vector.Gpu);
		if (vector.Length == 1)
			return new VectorInt([0], vector.Columns, gpu: vector.Gpu);
		if (vector.Is2D())
			return SortAlgorithms.Argsort2DDescX(vector, mode);
		return SortAlgorithms.Argsort1DDescX(vector, mode);
	}

	internal static void ArgsortAscXIP(VectorComplex64 vector, VectorInt indices, ComplexSortMode mode)
	{
		Validate(mode);
		Guard.MatchingGpuPairIndices(vector, indices);
		if (vector.Length == 0)
			return;
		if (vector.Is2D())
			SortAlgorithms.Argsort2DAscXIP(vector, indices, mode);
		else
			SortAlgorithms.Argsort1DAscXIP(vector, indices, mode);
	}

	internal static void ArgsortDescXIP(VectorComplex64 vector, VectorInt indices, ComplexSortMode mode)
	{
		Validate(mode);
		Guard.MatchingGpuPairIndices(vector, indices);
		if (vector.Length == 0)
			return;
		if (vector.Is2D())
			SortAlgorithms.Argsort2DDescXIP(vector, indices, mode);
		else
			SortAlgorithms.Argsort1DDescXIP(vector, indices, mode);
	}

	static void Validate(ComplexSortMode mode)
	{
		if (mode is not (ComplexSortMode.Magnitude or ComplexSortMode.Complex or
			ComplexSortMode.Real or ComplexSortMode.Imaginary))
			throw new ArgumentOutOfRangeException(nameof(mode));
	}
}
