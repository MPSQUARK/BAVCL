using System;
using System.Numerics;
using BAVCL.Core.Helpers;
using BAVCL.Types;

namespace BAVCL.Modules.Masking;

/// <summary>CPU compaction: broadcast shapes, element iteration, filter/partition/mask/where.</summary>
internal static class MaskCompactionCpu
{
	internal static (Shape VectorShape, Shape MaskShape, Shape OutputShape) ForFilter(Shape vectorShape, Shape maskShape)
	{
		Shape outputShape = vectorShape.BroadcastWith(maskShape);
		return (vectorShape, maskShape, outputShape);
	}

	internal static bool IsRowBroadcastMask(Shape maskShape, Shape outputShape) =>
		maskShape.Rows == 1 && outputShape.Rows > 1;

	internal static Vector Filter(Vector vector, Mask mask, Upload upload = Upload.Now)
	{
		float[] data = Filter(vector.RetrieveReadOnlySpan(), mask, vector.Shape());
		return new Vector(data, columns: 0, upload: upload, gpu: vector.Gpu);
	}

	internal static VectorInt Filter(VectorInt vector, Mask mask, Upload upload = Upload.Now)
	{
		int[] data = Filter(vector.RetrieveReadOnlySpan(), mask, vector.Shape());
		return new VectorInt(data, columns: 0, upload: upload, gpu: vector.Gpu);
	}

	internal static VectorComplex64 Filter(VectorComplex64 vector, Mask mask, Upload upload = Upload.Now)
	{
		Complex[] data = Filter(vector.RetrieveReadOnlySpan(), mask, vector.Shape());
		return new VectorComplex64(data, columns: 0, upload: upload, gpu: vector.Gpu);
	}

	internal static (Vector TrueLanes, Vector FalseLanes) Partition(Vector vector, Mask mask, Upload upload = Upload.Now)
	{
		(float[] trueData, float[] falseData) = Partition(vector.ToArray(), mask, vector.Shape());
		return (
			new Vector(trueData, columns: 0, upload: upload, gpu: vector.Gpu),
			new Vector(falseData, columns: 0, upload: upload, gpu: vector.Gpu));
	}

	internal static (VectorInt TrueLanes, VectorInt FalseLanes) Partition(VectorInt vector, Mask mask, Upload upload = Upload.Now)
	{
		(int[] trueData, int[] falseData) = Partition(vector.ToArray(), mask, vector.Shape());
		return (
			new VectorInt(trueData, columns: 0, upload: upload, gpu: vector.Gpu),
			new VectorInt(falseData, columns: 0, upload: upload, gpu: vector.Gpu));
	}

	internal static (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) Partition(
		VectorComplex64 vector,
		Mask mask,
		Upload upload = Upload.Now)
	{
		(Complex[] trueData, Complex[] falseData) = Partition(vector.ToArray(), mask, vector.Shape());
		return (
			new VectorComplex64(trueData, columns: 0, upload: upload, gpu: vector.Gpu),
			new VectorComplex64(falseData, columns: 0, upload: upload, gpu: vector.Gpu));
	}

	internal static VectorInt Where(Mask mask, Upload upload = Upload.Now)
	{
		Shape maskShape = mask.Shape();
		if (maskShape.Rows == 1 && maskShape.Cols > 0)
		{
			int[] rowIndices = WhereSingleRowMaskIndices(mask, maskShape);
			if (rowIndices.Length == 0)
				return new VectorInt(0, columns: 0, gpu: mask.Gpu);

			return new VectorInt(rowIndices, columns: 0, upload: upload, gpu: mask.Gpu);
		}

		int[] indices = SelectedSourceIndices(mask, maskShape, maskShape, maskShape, selected: true);
		return new VectorInt(indices, columns: 0, upload: upload, gpu: mask.Gpu);
	}

	internal static VectorComplex64 Mask(VectorComplex64 vector, Mask mask, Complex fill = default, Upload upload = Upload.Now)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilter(vector.Shape(), mask.Shape());
		if (outputShape.ElementCount == 0)
			return new VectorComplex64(0, outputShape.ToStorageColumns(), gpu: vector.Gpu);

		Complex[] output = new Complex[outputShape.ElementCount];
		FillMasked(
			output,
			outputShape,
			BroadcastStrides.For(vectorShape),
			BroadcastStrides.For(maskShape),
			mask.RetrieveReadOnlySpan(),
			vector.ToArray(),
			fill);

		return new VectorComplex64(output, outputShape.ToStorageColumns(), upload: upload, gpu: vector.Gpu);
	}

	static int[] SelectedColumns(Mask mask, Shape maskShape, int cols)
	{
		BroadcastStrides maskStrides = BroadcastStrides.For(maskShape);
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();

		int count = 0;
		for (int column = 0; column < cols; column++)
		{
			if (MaskBitOps.GetBit(words, maskStrides.IndexOf(0, column)))
				count++;
		}

		int[] columns = new int[count];
		count = 0;
		for (int column = 0; column < cols; column++)
		{
			if (MaskBitOps.GetBit(words, maskStrides.IndexOf(0, column)))
				columns[count++] = column;
		}

		return columns;
	}

	static MaskColumnTemplate BuildColumnTemplate(Mask mask, Shape maskShape, int cols)
	{
		BroadcastStrides maskStrides = BroadcastStrides.For(maskShape);
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();

		int trueCount = 0;
		for (int column = 0; column < cols; column++)
		{
			if (MaskBitOps.GetBit(words, maskStrides.IndexOf(0, column)))
				trueCount++;
		}

		int[] trueColumns = new int[trueCount];
		int[] falseColumns = new int[cols - trueCount];
		trueCount = 0;
		int falseCount = 0;
		for (int column = 0; column < cols; column++)
		{
			if (MaskBitOps.GetBit(words, maskStrides.IndexOf(0, column)))
				trueColumns[trueCount++] = column;
			else
				falseColumns[falseCount++] = column;
		}

		return new MaskColumnTemplate(trueColumns, falseColumns);
	}

	internal static T[] Gather<T>(ReadOnlySpan<T> input, ReadOnlySpan<int> sourceIndices) where T : unmanaged
	{
		T[] output = new T[sourceIndices.Length];
		for (int i = 0; i < sourceIndices.Length; i++)
			output[i] = input[sourceIndices[i]];

		return output;
	}

	internal static void FillMasked<T>(
		T[] output,
		Shape outputShape,
		BroadcastStrides vectorStrides,
		BroadcastStrides maskStrides,
		ReadOnlySpan<int> maskWords,
		T[] input,
		T fill) where T : unmanaged
	{
		int cols = outputShape.Cols;
		int write = 0;
		for (int linear = 0; linear < outputShape.ElementCount; linear++)
		{
			int row = linear / cols;
			int column = linear % cols;
			int vectorIndex = vectorStrides.IndexOf(row, column);
			int maskIndex = maskStrides.IndexOf(row, column);
			output[write++] = MaskBitOps.GetBit(maskWords, maskIndex)
				? input[vectorIndex]
				: fill;
		}
	}

	internal static T[] Filter<T>(ReadOnlySpan<T> input, Mask mask, Shape vectorShape) where T : unmanaged
	{
		Shape maskShape = mask.Shape();
		Shape outputShape = vectorShape.BroadcastWith(maskShape);
		if (IsRowBroadcastMask(maskShape, outputShape))
			return FilterBroadcast(
				input,
				BroadcastStrides.For(vectorShape),
				outputShape.Rows,
				SelectedColumns(mask, maskShape, outputShape.Cols));

		int[] indices = SelectedSourceIndices(mask, maskShape, vectorShape, outputShape, selected: true);
		return Gather(input, indices);
	}

	internal static (T[] True, T[] False) Partition<T>(T[] input, Mask mask, Shape vectorShape) where T : unmanaged
	{
		Shape maskShape = mask.Shape();
		Shape outputShape = vectorShape.BroadcastWith(maskShape);
		int elementCount = outputShape.ElementCount;
		if (elementCount == 0)
			return ([], []);

		if (UsesAlignedLinearCompaction(mask, maskShape, vectorShape, outputShape))
			return PartitionAlignedLinear(input, mask, elementCount);

		if (IsRowBroadcastMask(maskShape, outputShape))
		{
			MaskColumnTemplate template = BuildColumnTemplate(mask, maskShape, outputShape.Cols);
			return PartitionBroadcast(input, BroadcastStrides.For(vectorShape), outputShape.Rows, template);
		}

		return PartitionGeneral(input, mask, outputShape, vectorShape, maskShape);
	}

	internal static T[] FilterBroadcast<T>(
		ReadOnlySpan<T> input,
		BroadcastStrides vectorStrides,
		int rows,
		ReadOnlySpan<int> columns) where T : unmanaged
	{
		int outCount = columns.Length * rows;
		if (outCount == 0)
			return [];

		T[] output = new T[outCount];
		int write = 0;
		for (int row = 0; row < rows; row++)
		{
			for (int i = 0; i < columns.Length; i++)
				output[write++] = input[vectorStrides.IndexOf(row, columns[i])];
		}

		return output;
	}

	internal static (T[] True, T[] False) PartitionBroadcast<T>(
		ReadOnlySpan<T> input,
		BroadcastStrides vectorStrides,
		int rows,
		MaskColumnTemplate template) where T : unmanaged
	{
		ReadOnlySpan<int> trueCols = template.TrueColumns;
		ReadOnlySpan<int> falseCols = template.FalseColumns;
		T[] trueOutput = new T[trueCols.Length * rows];
		T[] falseOutput = new T[falseCols.Length * rows];
		int trueWrite = 0;
		int falseWrite = 0;

		for (int row = 0; row < rows; row++)
		{
			for (int i = 0; i < trueCols.Length; i++)
				trueOutput[trueWrite++] = input[vectorStrides.IndexOf(row, trueCols[i])];

			for (int i = 0; i < falseCols.Length; i++)
				falseOutput[falseWrite++] = input[vectorStrides.IndexOf(row, falseCols[i])];
		}

		return (trueOutput, falseOutput);
	}

	internal static (T[] True, T[] False) PartitionGeneral<T>(
		T[] input,
		Mask mask,
		Shape outputShape,
		Shape vectorShape,
		Shape maskShape) where T : unmanaged
	{
		int elementCount = outputShape.ElementCount;
		T[] trueScratch = new T[elementCount];
		T[] falseScratch = new T[elementCount];
		BroadcastStrides vectorStrides = BroadcastStrides.For(vectorShape);
		BroadcastStrides maskStrides = BroadcastStrides.For(maskShape);
		ReadOnlySpan<int> maskWords = mask.RetrieveReadOnlySpan();
		int trueWrite = 0;
		int falseWrite = 0;
		int cols = outputShape.Cols;

		for (int linear = 0; linear < elementCount; linear++)
		{
			int row = linear / cols;
			int column = linear % cols;
			T value = input[vectorStrides.IndexOf(row, column)];
			bool selected = MaskBitOps.GetBit(maskWords, maskStrides.IndexOf(row, column));
			if (selected)
				trueScratch[trueWrite++] = value;
			else
				falseScratch[falseWrite++] = value;
		}

		return (trueScratch[..trueWrite], falseScratch[..falseWrite]);
	}

	internal static (T[] True, T[] False) PartitionAlignedLinear<T>(
		ReadOnlySpan<T> input,
		Mask mask,
		int elementCount) where T : unmanaged
	{
		int trueCount = MaskCpu.CountTrue(mask);
		T[] trueOutput = new T[trueCount];
		T[] falseOutput = new T[elementCount - trueCount];
		PartitionAlignedLinear(input, mask.RetrieveReadOnlySpan(), elementCount, trueOutput, falseOutput);
		return (trueOutput, falseOutput);
	}

	internal static void PartitionAlignedLinear<T>(
		ReadOnlySpan<T> input,
		ReadOnlySpan<int> maskWords,
		int elementCount,
		Span<T> trueOutput,
		Span<T> falseOutput) where T : unmanaged
	{
		int trueWrite = 0;
		int falseWrite = 0;
		int vectorSize = Vector<T>.Count;
		int index = 0;
		int lastWord = maskWords.Length - 1;
		uint tailMask = (uint)MaskBitOps.TailMask(elementCount);

		for (int wordIndex = 0; wordIndex <= lastWord && index < elementCount; wordIndex++)
		{
			uint word = (uint)maskWords[wordIndex];
			if (wordIndex == lastWord)
				word &= tailMask;

			int bitsInWord = wordIndex == lastWord ? elementCount - index : Math.Min(32, elementCount - index);

			if (word == 0)
			{
				CopyRun(input, falseOutput, ref falseWrite, index, bitsInWord, vectorSize);
				index += bitsInWord;
				continue;
			}

			uint fullMask = wordIndex == lastWord ? tailMask : 0xFFFFFFFFu;
			if (word == fullMask)
			{
				CopyRun(input, trueOutput, ref trueWrite, index, bitsInWord, vectorSize);
				index += bitsInWord;
				continue;
			}

			for (int bit = 0; bit < bitsInWord; bit++)
			{
				T value = input[index];
				if ((word & (1u << bit)) != 0)
					trueOutput[trueWrite++] = value;
				else
					falseOutput[falseWrite++] = value;

				index++;
			}
		}
	}

	static void CopyRun<T>(
		ReadOnlySpan<T> input,
		Span<T> output,
		ref int write,
		int startIndex,
		int length,
		int vectorSize) where T : unmanaged
	{
		int i = 0;
		for (; i <= length - vectorSize; i += vectorSize)
		{
			Vector<T> chunk = new(input.Slice(startIndex + i, vectorSize));
			chunk.CopyTo(output.Slice(write, vectorSize));
			write += vectorSize;
		}

		for (; i < length; i++)
			output[write++] = input[startIndex + i];
	}

	internal static int[] SelectedSourceIndices(Mask mask, Shape maskShape, Shape vectorShape, Shape outputShape, bool selected)
	{
		int[] indices = new int[outputShape.ElementCount];
		BroadcastStrides vectorStrides = BroadcastStrides.For(vectorShape);
		BroadcastStrides maskStrides = BroadcastStrides.For(maskShape);
		ReadOnlySpan<int> maskWords = mask.RetrieveReadOnlySpan();
		int count = 0;
		int cols = outputShape.Cols;

		for (int linear = 0; linear < outputShape.ElementCount; linear++)
		{
			int row = linear / cols;
			int column = linear % cols;
			if (MaskBitOps.GetBit(maskWords, maskStrides.IndexOf(row, column)) != selected)
				continue;

			indices[count++] = vectorStrides.IndexOf(row, column);
		}

		return indices[..count];
	}

	internal static int[] WhereSingleRowMaskIndices(Mask mask, Shape maskShape)
	{
		int cols = maskShape.Cols;
		BroadcastStrides maskStrides = BroadcastStrides.For(maskShape);
		ReadOnlySpan<int> words = mask.RetrieveReadOnlySpan();

		int[] indices = new int[cols];
		int write = 0;
		for (int column = 0; column < cols; column++)
		{
			int linear = maskStrides.IndexOf(0, column);
			if (MaskBitOps.GetBit(words, linear))
				indices[write++] = linear;
		}

		return indices[..write];
	}

	internal static bool UsesAlignedLinearCompaction(Mask mask, Shape maskShape, Shape vectorShape, Shape outputShape) =>
		maskShape.MatchesDimensions(outputShape)
		&& vectorShape.MatchesDimensions(outputShape)
		&& mask.ElementCount == outputShape.ElementCount;

	internal static Vector Mask(Vector vector, Mask mask, float fill = 0f, Upload upload = Upload.Now)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilter(vector.Shape(), mask.Shape());
		if (outputShape.ElementCount == 0)
			return new Vector(0, outputShape.ToStorageColumns(), gpu: vector.Gpu);

		float[] output = new float[outputShape.ElementCount];
		FillMasked(
			output,
			outputShape,
			BroadcastStrides.For(vectorShape),
			BroadcastStrides.For(maskShape),
			mask.RetrieveReadOnlySpan(),
			vector.ToArray(),
			fill);

		return new Vector(output, outputShape.ToStorageColumns(), upload: upload, gpu: vector.Gpu);
	}

	internal static VectorInt Mask(VectorInt vector, Mask mask, int fill = 0, Upload upload = Upload.Now)
	{
		(Shape vectorShape, Shape maskShape, Shape outputShape) = ForFilter(vector.Shape(), mask.Shape());
		if (outputShape.ElementCount == 0)
			return new VectorInt(0, outputShape.ToStorageColumns(), gpu: vector.Gpu);

		int[] output = new int[outputShape.ElementCount];
		FillMasked(
			output,
			outputShape,
			BroadcastStrides.For(vectorShape),
			BroadcastStrides.For(maskShape),
			mask.RetrieveReadOnlySpan(),
			vector.ToArray(),
			fill);

		return new VectorInt(output, outputShape.ToStorageColumns(), upload: upload, gpu: vector.Gpu);
	}}
