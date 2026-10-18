using BAVCL.Types;

namespace BAVCL.Modules.Masking;

/// <summary>GPU compaction (<c>*X</c>): where indices, filter gather, partition gather.</summary>
internal static class MaskCompaction
{
	internal static VectorInt WhereX(Mask mask)
	{
		GPU gpu = mask.Gpu;
		Shape maskShape = mask.Shape();
		using SelectionLease selection = MaskGpuSelection.RunExclusiveScan(gpu, mask, maskShape, maskShape);
		int outputCount = selection.TrueCount;
		VectorInt output = new(gpu, outputCount, columns: 0);
		if (outputCount == 0)
			return output;

		using (GpuScope.Begin(output, [selection.FlagsPinTarget, selection.WritePosPinTarget]))
		{
			gpu.compactWriteIndices(
				gpu.DefaultStream,
				maskShape.ElementCount,
				output.GetBuffer().View,
				selection.Flags,
				selection.WritePos,
				maskShape.Cols,
				BroadcastStrides.For(maskShape));
			gpu.Synchronize();
		}

		return output;
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
		if (outputShape.ElementCount == 0)
			return new Vector(vector.Gpu, 0, columns: 0);

		GPU gpu = vector.Gpu;
		using SelectionLease selection = MaskGpuSelection.RunExclusiveScan(gpu, mask, maskShape, outputShape);

		int count = selection.TrueCount;
		Vector output = new(gpu, count, columns: 0);
		if (count == 0)
			return output;

		using (GpuScope.Begin(output, [vector, selection.FlagsPinTarget, selection.WritePosPinTarget]))
		{
			gpu.compactGatherFloat(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				selection.Flags,
				selection.WritePos,
				outputShape.Cols,
				BroadcastStrides.For(vectorShape));
			gpu.Synchronize();
		}

		return output;
	}

	internal static VectorInt FilterX(VectorInt vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		if (outputShape.ElementCount == 0)
			return new VectorInt(vector.Gpu, 0, columns: 0);

		GPU gpu = vector.Gpu;
		using SelectionLease selection = MaskGpuSelection.RunExclusiveScan(gpu, mask, maskShape, outputShape);

		int count = selection.TrueCount;
		VectorInt output = new(gpu, count, columns: 0);
		if (count == 0)
			return output;

		using (GpuScope.Begin(output, [vector, selection.FlagsPinTarget, selection.WritePosPinTarget]))
		{
			gpu.compactGatherInt(
				gpu.DefaultStream,
				outputShape.ElementCount,
				output.GetBuffer().View,
				vector.GetBuffer().View,
				selection.Flags,
				selection.WritePos,
				outputShape.Cols,
				BroadcastStrides.For(vectorShape));
			gpu.Synchronize();
		}

		return output;
	}

	internal static (Vector TrueLanes, Vector FalseLanes) PartitionX(Vector vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		if (outputShape.ElementCount == 0)
			return (new Vector(vector.Gpu, 0, columns: 0), new Vector(vector.Gpu, 0, columns: 0));

		GPU gpu = vector.Gpu;
		using SelectionLease selection = MaskGpuSelection.RunExclusiveScan(gpu, mask, maskShape, outputShape);

		Vector trueOutput = new(gpu, selection.TrueCount, columns: 0);
		Vector falseOutput = new(gpu, selection.FalseCount, columns: 0);
		BroadcastStrides vectorStrides = BroadcastStrides.For(vectorShape);

		using (GpuScope.Begin(
			[trueOutput, falseOutput],
			[vector, selection.FlagsPinTarget, selection.WritePosPinTarget]))
		{
			gpu.compactGatherFloatPartition(
				gpu.DefaultStream,
				outputShape.ElementCount,
				trueOutput.GetBuffer().View,
				falseOutput.GetBuffer().View,
				vector.GetBuffer().View,
				selection.Flags,
				selection.WritePos,
				outputShape.Cols,
				vectorStrides);
			gpu.Synchronize();
		}

		return (trueOutput, falseOutput);
	}

	internal static (VectorInt TrueLanes, VectorInt FalseLanes) PartitionX(VectorInt vector, Mask mask)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilterGpu(vector, mask);
		if (outputShape.ElementCount == 0)
			return (new VectorInt(vector.Gpu, 0, columns: 0), new VectorInt(vector.Gpu, 0, columns: 0));

		GPU gpu = vector.Gpu;
		using SelectionLease selection = MaskGpuSelection.RunExclusiveScan(gpu, mask, maskShape, outputShape);

		VectorInt trueOutput = new(gpu, selection.TrueCount, columns: 0);
		VectorInt falseOutput = new(gpu, selection.FalseCount, columns: 0);
		BroadcastStrides vectorStrides = BroadcastStrides.For(vectorShape);

		using (GpuScope.Begin(
			[trueOutput, falseOutput],
			[vector, selection.FlagsPinTarget, selection.WritePosPinTarget]))
		{
			gpu.compactGatherIntPartition(
				gpu.DefaultStream,
				outputShape.ElementCount,
				trueOutput.GetBuffer().View,
				falseOutput.GetBuffer().View,
				vector.GetBuffer().View,
				selection.Flags,
				selection.WritePos,
				outputShape.Cols,
				vectorStrides);
			gpu.Synchronize();
		}

		return (trueOutput, falseOutput);
	}
}
