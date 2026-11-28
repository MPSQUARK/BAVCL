using BAVCL.Core;
using BAVCL.Core.Memory;
using BAVCL.Types;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

/// <summary>GPU compaction (<c>*X</c>): where indices, filter gather, partition gather.</summary>
internal static class MaskCompaction
{
	internal static VectorInt WhereX(Mask mask)
	{
		GPU gpu = mask.Gpu;
		Shape maskShape = mask.Shape();
		int elementCount = maskShape.ElementCount;
		if (elementCount == 0)
			return new VectorInt(gpu, 0, columns: 0);

		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			int trueCount = MaskGpuSelection.ExclusivePrefixCount(
				gpu, mask, maskShape, maskShape, flags.View, writePos.View);
			if (trueCount == 0)
				return new VectorInt(gpu, 0, columns: 0);

			VectorInt output = new(gpu, trueCount, columns: 0);
			using (GpuScope.Begin(output))
			{
				gpu.compactWriteIndices(
					gpu.DefaultStream,
					elementCount,
					output.GetBuffer().View,
					flags.View,
					writePos.View,
					maskShape.Cols,
					BroadcastStrides.For(maskShape));
				gpu.Synchronize();
			}

			return output;
		}
	}

	static (Shape VectorShape, Shape MaskShape, Shape OutputShape) ForFilterGpu<T>(VectorBase<T> vector, Mask mask)
		where T : unmanaged
	{
		Guard.MatchingGpu(vector, mask);
		return MaskCompactionCpu.ForFilter(vector.Shape(), mask.Shape());
	}

	internal static Vector FilterX(Vector vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		int elementCount = outputShape.ElementCount;
		if (elementCount == 0)
			return new Vector(vector.Gpu, 0, columns: 0);

		GPU gpu = vector.Gpu;
		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			int count = MaskGpuSelection.ExclusivePrefixCount(
				gpu, mask, maskShape, outputShape, flags.View, writePos.View);
			if (count == 0)
				return new Vector(gpu, 0, columns: 0);

			Vector output = new(gpu, count, columns: 0);
			using (GpuScope.Begin(output, vector))
			{
				gpu.compactGatherFloat(
					gpu.DefaultStream,
					elementCount,
					output.GetBuffer().View,
					vector.GetBuffer().View,
					flags.View,
					writePos.View,
					outputShape.Cols,
					BroadcastStrides.For(vectorShape));
				gpu.Synchronize();
			}

			return output;
		}
	}

	internal static VectorInt FilterX(VectorInt vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		int elementCount = outputShape.ElementCount;
		if (elementCount == 0)
			return new VectorInt(vector.Gpu, 0, columns: 0);

		GPU gpu = vector.Gpu;
		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			int count = MaskGpuSelection.ExclusivePrefixCount(
				gpu, mask, maskShape, outputShape, flags.View, writePos.View);
			if (count == 0)
				return new VectorInt(gpu, 0, columns: 0);

			VectorInt output = new(gpu, count, columns: 0);
			using (GpuScope.Begin(output, vector))
			{
				gpu.compactGatherInt(
					gpu.DefaultStream,
					elementCount,
					output.GetBuffer().View,
					vector.GetBuffer().View,
					flags.View,
					writePos.View,
					outputShape.Cols,
					BroadcastStrides.For(vectorShape));
				gpu.Synchronize();
			}

			return output;
		}
	}

	internal static VectorComplex64 FilterX(VectorComplex64 vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		int elementCount = outputShape.ElementCount;
		if (elementCount == 0)
			return new VectorComplex64(vector.Gpu, 0, columns: 0);

		GPU gpu = vector.Gpu;
		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			int count = MaskGpuSelection.ExclusivePrefixCount(
				gpu, mask, maskShape, outputShape, flags.View, writePos.View);
			if (count == 0)
				return new VectorComplex64(gpu, 0, columns: 0);

			VectorComplex64 output = new(gpu, count, columns: 0);
			using (GpuScope.Begin(output, vector))
			{
				gpu.compactGatherComplex(
					gpu.DefaultStream,
					elementCount,
					output.GetBuffer().View,
					vector.GetBuffer().View,
					flags.View,
					writePos.View,
					outputShape.Cols,
					BroadcastStrides.For(vectorShape));
				gpu.Synchronize();
			}

			return output;
		}
	}

	internal static (Vector TrueLanes, Vector FalseLanes) PartitionX(Vector vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		int elementCount = outputShape.ElementCount;
		if (elementCount == 0)
			return (new Vector(vector.Gpu, 0, columns: 0), new Vector(vector.Gpu, 0, columns: 0));

		GPU gpu = vector.Gpu;
		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			int trueCount = MaskGpuSelection.ExclusivePrefixCount(
				gpu, mask, maskShape, outputShape, flags.View, writePos.View);
			int falseCount = elementCount - trueCount;
			Vector trueOutput = new(gpu, trueCount, columns: 0);
			Vector falseOutput = new(gpu, falseCount, columns: 0);
			using (GpuScope.Begin([trueOutput, falseOutput], [vector]))
			{
				gpu.compactGatherFloatPartition(
					gpu.DefaultStream,
					elementCount,
					trueOutput.GetBuffer().View,
					falseOutput.GetBuffer().View,
					vector.GetBuffer().View,
					flags.View,
					writePos.View,
					outputShape.Cols,
					BroadcastStrides.For(vectorShape));
				gpu.Synchronize();
			}

			return (trueOutput, falseOutput);
		}
	}

	internal static (VectorInt TrueLanes, VectorInt FalseLanes) PartitionX(VectorInt vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		int elementCount = outputShape.ElementCount;
		if (elementCount == 0)
			return (new VectorInt(vector.Gpu, 0, columns: 0), new VectorInt(vector.Gpu, 0, columns: 0));

		GPU gpu = vector.Gpu;
		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			int trueCount = MaskGpuSelection.ExclusivePrefixCount(
				gpu, mask, maskShape, outputShape, flags.View, writePos.View);
			int falseCount = elementCount - trueCount;
			VectorInt trueOutput = new(gpu, trueCount, columns: 0);
			VectorInt falseOutput = new(gpu, falseCount, columns: 0);
			using (GpuScope.Begin([trueOutput, falseOutput], [vector]))
			{
				gpu.compactGatherIntPartition(
					gpu.DefaultStream,
					elementCount,
					trueOutput.GetBuffer().View,
					falseOutput.GetBuffer().View,
					vector.GetBuffer().View,
					flags.View,
					writePos.View,
					outputShape.Cols,
					BroadcastStrides.For(vectorShape));
				gpu.Synchronize();
			}

			return (trueOutput, falseOutput);
		}
	}

	internal static (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) PartitionX(VectorComplex64 vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		int elementCount = outputShape.ElementCount;
		if (elementCount == 0)
			return (new VectorComplex64(vector.Gpu, 0, columns: 0), new VectorComplex64(vector.Gpu, 0, columns: 0));

		GPU gpu = vector.Gpu;
		using BufferEntity<int> flags = BufferPools.For(gpu).Int.Rent(elementCount);
		using BufferEntity<int> writePos = BufferPools.For(gpu).Int.Rent(elementCount);
		using (GpuScope.Begin([flags.PinTarget, writePos.PinTarget], [mask]))
		{
			int trueCount = MaskGpuSelection.ExclusivePrefixCount(
				gpu, mask, maskShape, outputShape, flags.View, writePos.View);
			int falseCount = elementCount - trueCount;
			VectorComplex64 trueOutput = new(gpu, trueCount, columns: 0);
			VectorComplex64 falseOutput = new(gpu, falseCount, columns: 0);
			using (GpuScope.Begin([trueOutput, falseOutput], [vector]))
			{
				gpu.compactGatherComplexPartition(
					gpu.DefaultStream,
					elementCount,
					trueOutput.GetBuffer().View,
					falseOutput.GetBuffer().View,
					vector.GetBuffer().View,
					flags.View,
					writePos.View,
					outputShape.Cols,
					BroadcastStrides.For(vectorShape));
				gpu.Synchronize();
			}

			return (trueOutput, falseOutput);
		}
	}
}
