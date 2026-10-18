using System;
using BAVCL.Core.Helpers;
using BAVCL.Core.Memory;
using BAVCL.Modules.Statistics;
using BAVCL.Types;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

/// <summary>GPU mask: vector compare/fill, word aggregates, index search.</summary>
internal static class MaskGpu
{
	internal static int CountTrueX(Mask mask)
	{
		int wordCount = MaskBitOps.WordCount(mask.ElementCount);
		if (wordCount == 0)
			return 0;

		return RunWordPopcountPartialSum(mask, wordCount);
	}

	internal static bool AnyX(Mask mask)
	{
		int wordCount = MaskBitOps.WordCount(mask.ElementCount);
		if (wordCount == 0)
			return false;

		return RunWordAnyPartial(mask, wordCount);
	}

	internal static bool AllX(Mask mask)
	{
		if (mask.ElementCount == 0)
			return true;

		int wordCount = MaskBitOps.WordCount(mask.ElementCount);
		return RunWordAllPartialSum(mask, wordCount) == wordCount;
	}

	internal static int FirstTrueIndexX(Mask mask)
	{
		int wordCount = MaskBitOps.WordCount(mask.ElementCount);
		if (wordCount == 0)
			return -1;

		return RunWordFirstTruePartialIndex(mask, wordCount);
	}

	internal static int LastTrueIndexX(Mask mask)
	{
		int wordCount = MaskBitOps.WordCount(mask.ElementCount);
		if (wordCount == 0)
			return -1;

		return RunWordLastTruePartialIndex(mask, wordCount);
	}

	internal static Mask CompareX(Vector left, Vector right, VectorComparison comparison)
	{
		Guard.MatchingGpu(left, right);
		Shape leftShape = left.Shape();
		Shape rightShape = right.Shape();
		Shape outputShape = leftShape.BroadcastWith(rightShape);

		GPU gpu = left.Gpu;
		Mask output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, left, right))
		{
			gpu.compareMask(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				left.GetBuffer().View,
				right.GetBuffer().View,
				outputShape.Cols,
				BroadcastStrides.For(leftShape),
				BroadcastStrides.For(rightShape),
				new SpecializedValue<int>((int)comparison));
			gpu.Synchronize();
		}

		return output;
	}

	internal static Mask CompareX(VectorInt left, VectorInt right, VectorComparison comparison)
	{
		Guard.MatchingGpu(left, right);
		Shape leftShape = left.Shape();
		Shape rightShape = right.Shape();
		Shape outputShape = leftShape.BroadcastWith(rightShape);

		GPU gpu = left.Gpu;
		Mask output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, left, right))
		{
			gpu.compareMaskInt(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				left.GetBuffer().View,
				right.GetBuffer().View,
				outputShape.Cols,
				BroadcastStrides.For(leftShape),
				BroadcastStrides.For(rightShape),
				new SpecializedValue<int>((int)comparison));
			gpu.Synchronize();
		}

		return output;
	}

	internal static Mask CompareX(Vector vector, float scalar, VectorComparison comparison)
	{
		GPU gpu = vector.Gpu;
		Mask output = new(gpu, vector.Length, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			gpu.compareScalarMask(
				gpu.DefaultStream,
				vector.Length,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				scalar,
				new SpecializedValue<int>((int)comparison));
			gpu.Synchronize();
		}

		return output;
	}

	internal static Mask CompareX(VectorInt vector, int scalar, VectorComparison comparison)
	{
		GPU gpu = vector.Gpu;
		Mask output = new(gpu, vector.Length, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			gpu.compareScalarMaskInt(
				gpu.DefaultStream,
				vector.Length,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				scalar,
				new SpecializedValue<int>((int)comparison));
			gpu.Synchronize();
		}

		return output;
	}

	internal static Vector MaskX(Vector vector, Mask mask, float fill)
	{
		Guard.MatchingGpu(vector, mask);
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = vectorShape.BroadcastWith(maskShape);

		GPU gpu = vector.Gpu;
		Vector output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, vector, mask))
		{
			gpu.maskFilter(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				mask.GetBuffer().View,
				fill,
				outputShape.Cols,
				BroadcastStrides.For(vectorShape),
				BroadcastStrides.For(maskShape));
			gpu.Synchronize();
		}

		return output;
	}

	internal static VectorInt MaskX(VectorInt vector, Mask mask, int fill)
	{
		Guard.MatchingGpu(vector, mask);
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = vectorShape.BroadcastWith(maskShape);

		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, outputShape.ElementCount, outputShape.ToStorageColumns());

		using (GpuScope.Begin(output, vector, mask))
		{
			gpu.maskFilterInt(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				mask.GetBuffer().View,
				fill,
				outputShape.Cols,
				BroadcastStrides.For(vectorShape),
				BroadcastStrides.For(maskShape));
			gpu.Synchronize();
		}

		return output;
	}

	static int RunWordPopcountPartialSum(Mask mask, int wordCount)
	{
		GPU gpu = mask.Gpu;
		int lastWord = wordCount - 1;
		int tailMask = MaskBitOps.TailMask(mask.ElementCount);

		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(wordCount);
		using (GpuScope.Begin(partial.PinTarget, mask))
		{
			gpu.maskPopcountPartial(
				gpu.DefaultStream,
				wordCount,
				partial.View,
				mask.GetBuffer().View,
				lastWord,
				tailMask);
			gpu.Synchronize();
			return SumPartialInts(partial.ReadRentedSpan());
		}
	}

	static bool RunWordAnyPartial(Mask mask, int wordCount)
	{
		GPU gpu = mask.Gpu;
		int lastWord = wordCount - 1;
		int tailMask = MaskBitOps.TailMask(mask.ElementCount);

		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(wordCount);
		using (GpuScope.Begin(partial.PinTarget, mask))
		{
			gpu.maskAnyPartial(
				gpu.DefaultStream,
				wordCount,
				partial.View,
				mask.GetBuffer().View,
				lastWord,
				tailMask);
			gpu.Synchronize();
			return AnyPartialLaneSet(partial.ReadRentedSpan());
		}
	}

	static int RunWordAllPartialSum(Mask mask, int wordCount)
	{
		GPU gpu = mask.Gpu;
		int lastWord = wordCount - 1;
		int tailMask = MaskBitOps.TailMask(mask.ElementCount);

		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(wordCount);
		using (GpuScope.Begin(partial.PinTarget, mask))
		{
			gpu.maskAllPartial(
				gpu.DefaultStream,
				wordCount,
				partial.View,
				mask.GetBuffer().View,
				lastWord,
				tailMask);
			gpu.Synchronize();
			return SumPartialInts(partial.ReadRentedSpan());
		}
	}

	static int RunWordFirstTruePartialIndex(Mask mask, int wordCount)
	{
		GPU gpu = mask.Gpu;
		int lastWord = wordCount - 1;
		int tailMask = MaskBitOps.TailMask(mask.ElementCount);

		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(wordCount);
		using (GpuScope.Begin(partial.PinTarget, mask))
		{
			gpu.maskFirstTrueIndex(
				gpu.DefaultStream,
				wordCount,
				mask.GetBuffer().View,
				partial.View,
				lastWord,
				tailMask);
			gpu.Synchronize();
			return FoldFirstTrueIndex(partial.ReadRentedSpan());
		}
	}

	static int RunWordLastTruePartialIndex(Mask mask, int wordCount)
	{
		GPU gpu = mask.Gpu;
		int lastWord = wordCount - 1;
		int tailMask = MaskBitOps.TailMask(mask.ElementCount);

		using BufferEntity<int> partial = BufferPools.For(gpu).Int.Rent(wordCount);
		using (GpuScope.Begin(partial.PinTarget, mask))
		{
			gpu.maskLastTrueIndex(
				gpu.DefaultStream,
				wordCount,
				mask.GetBuffer().View,
				partial.View,
				lastWord,
				tailMask);
			gpu.Synchronize();
			return CpuSimdReduce.MaxInt(partial.ReadRentedSpan());
		}
	}

	static int SumPartialInts(ReadOnlySpan<int> partial)
	{
		int total = 0;
		for (int i = 0; i < partial.Length; i++)
			total += partial[i];

		return total;
	}

	static bool AnyPartialLaneSet(ReadOnlySpan<int> partial)
	{
		for (int i = 0; i < partial.Length; i++)
		{
			if (partial[i] != 0)
				return true;
		}

		return false;
	}

	static int FoldFirstTrueIndex(ReadOnlySpan<int> values)
	{
		int best = int.MaxValue;
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i] < best)
				best = values[i];
		}

		return best == int.MaxValue ? -1 : best;
	}
}
