using System;
using System.Numerics;
using ILGPU;
using ILGPU.Algorithms;
using BAVCL.Core.Exceptions;
using BAVCL.Core.Kernels;
using BAVCL.Core.Helpers;
using BAVCL.Core.Helpers.Numerics;
using BAVCL.Core.Memory;
using BAVCL.Types;

namespace BAVCL.GpuAlgorithms;

internal static class GlobalReduceAlgorithms
{
	internal static float Sum(Vector vector)
	{
		if (vector.Length == 0)
			return 0f;

		return SumGroupedCompensatedFloat(vector);
	}

	internal static float Sum(VectorInt vector)
	{
		if (vector.Length == 0)
			return 0f;

		return SumGroupedInt(vector);
	}

	internal static float Dot(Vector left, Vector right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(Dot), left.Length, right.Length);

		if (left.Length == 0)
			return 0f;

		return DotFloat(left, right);
	}

	internal static float Dot(VectorInt left, VectorInt right)
	{
		if (left.Length != right.Length)
			throw new LengthMismatchException(nameof(Dot), left.Length, right.Length);

		if (left.Length == 0)
			return 0f;

		return DotInt(left, right);
	}

	internal static float Dot(Vector vector, float scalar)
	{
		if (vector.Length == 0)
			return 0f;

		return scalar * Sum(vector);
	}

	internal static float Dot(VectorInt vector, int scalar)
	{
		if (vector.Length == 0)
			return 0f;

		return scalar * Sum(vector);
	}

	internal static float Min(Vector vector)
	{
		if (vector.Length == 0)
			return float.NaN;

		return ReduceFloat(vector, vector.Gpu.ReduceMinFloat);
	}

	internal static int Min(VectorInt vector)
	{
		Guard.IsNotEmpty(vector.Length);
		return ReduceInt(vector, vector.Gpu.ReduceMinInt32);
	}

	internal static float Max(Vector vector)
	{
		if (vector.Length == 0)
			return float.NaN;

		return ReduceFloat(vector, vector.Gpu.ReduceMaxFloat);
	}

	internal static int Max(VectorInt vector)
	{
		Guard.IsNotEmpty(vector.Length);
		return ReduceInt(vector, vector.Gpu.ReduceMaxInt32);
	}

	internal static float RangeX(Vector vector)
	{
		MinMax<float> pair = MinMaxX(vector);
		return pair.Max - pair.Min;
	}

	internal static int RangeX(VectorInt vector)
	{
		MinMax<int> pair = MinMaxX(vector);
		return pair.Max - pair.Min;
	}

	internal static float Var(Vector vector)
	{
		if (vector.Length == 0)
			return 0f;

		return VarianceMomentsFloatGrouped(vector).PopulationVariance();
	}

	internal static float SampleVar(Vector vector)
	{
		if (vector.Length == 0)
			return 0f;

		return VarianceMomentsFloatGrouped(vector).SampleVariance();
	}

	internal static float Var(VectorInt vector)
	{
		if (vector.Length == 0)
			return 0f;

		return VarianceMomentsIntGrouped(vector).PopulationVariance();
	}

	internal static float SampleVar(VectorInt vector)
	{
		if (vector.Length == 0)
			return 0f;

		return VarianceMomentsIntGrouped(vector).SampleVariance();
	}

	internal static float SumSquaredDiffInt(VectorInt vector, float mean)
	{
		Guard.IsNotEmpty(vector.Length);
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(numGroups);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.sumSquaredDiffIntGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				mean,
				vector.Length);
			gpu.Synchronize();
		}

		return CompensatedFold(partial.ReadRentedSpan());
	}

	internal static bool All(Vector vector)
	{
		if (vector.Length == 0)
			return true;

		return AllNonZeroFloat(vector);
	}

	internal static bool All(VectorInt vector)
	{
		if (vector.Length == 0)
			return true;

		return AllNonZeroInt(vector);
	}

	internal static MinMax<float> MinMaxX(Vector vector)
	{
		if (vector.Length == 0)
			return new MinMax<float>(float.NaN, float.NaN);

		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		int partialLength = numGroups << 1;
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(partialLength);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.minMaxReduceFloatGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldGroupedMinMax(partial.ReadRentedSpan());
	}

	internal static MinMax<int> MinMaxX(VectorInt vector)
	{
		Guard.IsNotEmpty(vector.Length);
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		int partialLength = numGroups << 1;
		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(partialLength);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.minMaxReduceIntGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldGroupedMinMax(partial.ReadRentedSpan());
	}

	static float SumGroupedCompensatedFloat(Vector vector)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(numGroups);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.sumReduceFloatGroupedCompensatedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return CompensatedFold(partial.ReadRentedSpan());
	}

	static float SumGroupedInt(VectorInt vector)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(numGroups);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.sumReduceIntGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldGroupedIntToFloat(partial.ReadRentedSpan());
	}

	static float DotFloat(Vector left, Vector right)
	{
		GPU gpu = left.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(left.Length);
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(numGroups);
		using (GpuScope.Begin(partial.PinTarget, left, right))
		{
			gpu.dotReduceFloatGroupedCompensatedKernel(
				gpu.DefaultStream,
				config,
				left.GetBuffer().View,
				right.GetBuffer().View,
				partial.View,
				left.Length);
			gpu.Synchronize();
		}

		return CompensatedFold(partial.ReadRentedSpan());
	}

	static float DotInt(VectorInt left, VectorInt right)
	{
		GPU gpu = left.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(left.Length);
		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(numGroups);
		using (GpuScope.Begin(partial.PinTarget, left, right))
		{
			gpu.dotReduceIntGroupedKernel(
				gpu.DefaultStream,
				config,
				left.GetBuffer().View,
				right.GetBuffer().View,
				partial.View,
				left.Length);
			gpu.Synchronize();
		}

		return FoldGroupedIntToFloat(partial.ReadRentedSpan());
	}

	static bool AllNonZeroFloat(Vector vector)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(numGroups);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.allNonZeroFloatGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldAllNonZero(partial.ReadRentedSpan());
	}

	static bool AllNonZeroInt(VectorInt vector)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(numGroups);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.allNonZeroIntGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldAllNonZero(partial.ReadRentedSpan());
	}

	static float ReduceFloat(Vector vector, Reduction<float, Stride1D.Dense> reduction)
	{
		GPU gpu = vector.Gpu;
		using BufferEntity<float> output = BufferPools.For(gpu).Float.Rent(1);
		using (GpuScope.Begin(output.PinTarget, vector))
		{
			reduction(gpu.DefaultStream, vector.GetBuffer().View, output.View);
			gpu.Synchronize();
		}

		return output.ReadRentedSpan()[0];
	}

	static int ReduceInt(VectorInt vector, Reduction<int, Stride1D.Dense> reduction)
	{
		GPU gpu = vector.Gpu;
		using BufferEntity<int> output = BufferPools.For(gpu).Int.Rent(1);
		using (GpuScope.Begin(output.PinTarget, vector))
		{
			reduction(gpu.DefaultStream, vector.GetBuffer().View, output.View);
			gpu.Synchronize();
		}

		return output.ReadRentedSpan()[0];
	}

	static (KernelConfig config, int numGroups) ReduceLaunch(int length)
	{
		int numGroups = (length + KernelSharedMemory.GlobalReduceGroupSize - 1) / KernelSharedMemory.GlobalReduceGroupSize;
		return ((numGroups, KernelSharedMemory.GlobalReduceGroupSize), numGroups);
	}

	/// <summary>
	/// Each group stores ordered (lo, hi). Empty groups keep identity (lo &gt; hi) and are skipped.
	/// Because lo ≤ hi, one comparison against the running extrema is enough per bound.
	/// Identities: +∞/−∞ for IEEE types (via saturating convert), MaxValue/MinValue for integers.
	/// </summary>
	static MinMax<T> FoldGroupedMinMax<T>(ReadOnlySpan<T> packed)
		where T : unmanaged, INumber<T>
	{
		T min = T.CreateSaturating(double.PositiveInfinity);
		T max = T.CreateSaturating(double.NegativeInfinity);
		int numGroups = packed.Length >> 1;
		for (int group = 0; group < numGroups; group++)
		{
			int slot = group << 1;
			T lo = packed[slot];
			T hi = packed[slot + 1];
			if (lo > hi)
				continue;

			if (lo < min)
				min = lo;
			if (hi > max)
				max = hi;
		}

		return new MinMax<T>(min, max);
	}

	static float CompensatedFold(ReadOnlySpan<float> values)
	{
		double sum = 0d;
		double compensation = 0d;
		for (int i = 0; i < values.Length; i++)
			CompensatedSumOps.NeumaierAdd(ref sum, ref compensation, values[i]);

		return (float)(sum + compensation);
	}

	/// <summary>
	/// Folds grouped <c>int</c> partials from int sum/dot kernels into a <c>float</c> result.
	/// Accumulation is unchecked <c>int32</c> — overflow is the caller's responsibility.
	/// </summary>
	static float FoldGroupedIntToFloat(ReadOnlySpan<int> values)
	{
		int total = 0;
		for (int i = 0; i < values.Length; i++)
			total += values[i];

		return total;
	}

	static VarianceMoments VarianceMomentsFloatGrouped(Vector vector)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		int partialLength = numGroups * 3;
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(partialLength);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.varReduceFloatGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldGroupedVarianceMoments(partial.ReadRentedSpan());
	}

	static VarianceMoments VarianceMomentsIntGrouped(VectorInt vector)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		int partialLength = numGroups * 3;
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(partialLength);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			gpu.varReduceIntGroupedKernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldGroupedVarianceMoments(partial.ReadRentedSpan());
	}

	static VarianceMoments FoldGroupedVarianceMoments(ReadOnlySpan<float> triples)
	{
		int count = 0;
		float mean = 0f;
		float m2 = 0f;
		for (int i = 0; i + 2 < triples.Length; i += 3)
		{
			int groupCount = (int)triples[i];
			VarianceAccumOps.Merge(ref count, ref mean, ref m2, groupCount, triples[i + 1], triples[i + 2]);
		}

		return new VarianceMoments(count, mean, m2);
	}

	static bool FoldAllNonZero(ReadOnlySpan<int> groupFlags)
	{
		for (int i = 0; i < groupFlags.Length; i++)
		{
			if (groupFlags[i] != 0)
				return false;
		}

		return true;
	}
}
