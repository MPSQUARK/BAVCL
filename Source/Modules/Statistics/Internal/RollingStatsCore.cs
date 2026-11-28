using System;
using BAVCL.Modules.Scan;

namespace BAVCL.Modules.Statistics;

internal static class RollingStatsCore
{
	internal static Vector RollingSum1D(Vector vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanCore.CumSumInclusive1D(vector, upload);
		return ApplyRollingSum1D(vector, cum, window, upload);
	}

	internal static Vector RollingSum2D(Vector vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanCore.CumSumInclusive2D(vector, upload);
		return ApplyRollingSum2D(vector, cum, window, upload);
	}

	internal static VectorInt RollingSum1D(VectorInt vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanCore.CumSumInclusive1D(vector, upload);
		return ApplyRollingSum1D(vector, cum, window, upload);
	}

	internal static VectorInt RollingSum2D(VectorInt vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanCore.CumSumInclusive2D(vector, upload);
		return ApplyRollingSum2D(vector, cum, window, upload);
	}

	internal static Vector MovingAverage1D(Vector vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanCore.CumSumInclusive1D(vector, upload);
		return ApplyMovingAverage1D(vector, cum, window, upload);
	}

	internal static Vector MovingAverage2D(Vector vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanCore.CumSumInclusive2D(vector, upload);
		return ApplyMovingAverage2D(vector, cum, window, upload);
	}

	internal static Vector MovingAverage1D(VectorInt vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanCore.CumSumInclusive1D(vector, upload);
		VectorInt rolling = ApplyRollingSum1D(vector, cum, window, upload);
		return ConvertToAverage1D(rolling, window, upload);
	}

	internal static Vector MovingAverage2D(VectorInt vector, int window, Upload upload = Upload.Now)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanCore.CumSumInclusive2D(vector, upload);
		VectorInt rolling = ApplyRollingSum2D(vector, cum, window, upload);
		return ConvertToAverage2D(rolling, window, upload);
	}

	static Vector ApplyRollingSum1D(Vector vector, Vector cum, int window, Upload upload)
	{
		Vector output = new(vector.Length, vector.Columns, gpu: vector.Gpu);
		ReadOnlySpan<float> cumSpan = cum.RetrieveReadOnlySpan();

		using (output.CpuScope(upload))
			FillRollingSum1D(output.Value, cumSpan, vector.Length, window);

		return output;
	}

	static Vector ApplyRollingSum2D(Vector vector, Vector cum, int window, Upload upload)
	{
		Vector output = new(vector.Length, vector.Columns, gpu: vector.Gpu);
		ReadOnlySpan<float> cumSpan = cum.RetrieveReadOnlySpan();
		int cols = vector.ElementsPerRow();

		using (output.CpuScope(upload))
			FillRollingSum2D(output.Value, cumSpan, vector.Length, cols, window);

		return output;
	}

	static VectorInt ApplyRollingSum1D(VectorInt vector, VectorInt cum, int window, Upload upload)
	{
		VectorInt output = new(vector.Length, vector.Columns, gpu: vector.Gpu);
		ReadOnlySpan<int> cumSpan = cum.RetrieveReadOnlySpan();

		using (output.CpuScope(upload))
			FillRollingSum1D(output.Value, cumSpan, vector.Length, window);

		return output;
	}

	static VectorInt ApplyRollingSum2D(VectorInt vector, VectorInt cum, int window, Upload upload)
	{
		VectorInt output = new(vector.Length, vector.Columns, gpu: vector.Gpu);
		ReadOnlySpan<int> cumSpan = cum.RetrieveReadOnlySpan();
		int cols = vector.ElementsPerRow();

		using (output.CpuScope(upload))
			FillRollingSum2D(output.Value, cumSpan, vector.Length, cols, window);

		return output;
	}

	static Vector ApplyMovingAverage1D(Vector vector, Vector cum, int window, Upload upload)
	{
		Vector output = new(vector.Length, vector.Columns, gpu: vector.Gpu);
		ReadOnlySpan<float> cumSpan = cum.RetrieveReadOnlySpan();

		using (output.CpuScope(upload))
			FillMovingAverage1D(output.Value, cumSpan, vector.Length, window);

		return output;
	}

	static Vector ApplyMovingAverage2D(Vector vector, Vector cum, int window, Upload upload)
	{
		Vector output = new(vector.Length, vector.Columns, gpu: vector.Gpu);
		ReadOnlySpan<float> cumSpan = cum.RetrieveReadOnlySpan();
		int cols = vector.ElementsPerRow();

		using (output.CpuScope(upload))
			FillMovingAverage2D(output.Value, cumSpan, vector.Length, cols, window);

		return output;
	}

	static Vector ConvertToAverage1D(VectorInt rolling, int window, Upload upload)
	{
		Vector output = new(rolling.Length, rolling.Columns, gpu: rolling.Gpu);
		ReadOnlySpan<int> rollingSpan = rolling.RetrieveReadOnlySpan();

		using (output.CpuScope(upload))
			FillAverageFromRolling1D(output.Value, rollingSpan, rolling.Length, window);

		return output;
	}

	static Vector ConvertToAverage2D(VectorInt rolling, int window, Upload upload)
	{
		Vector output = new(rolling.Length, rolling.Columns, gpu: rolling.Gpu);
		ReadOnlySpan<int> rollingSpan = rolling.RetrieveReadOnlySpan();
		int cols = rolling.ElementsPerRow();

		using (output.CpuScope(upload))
			FillAverageFromRolling2D(output.Value, rollingSpan, rolling.Length, cols, window);

		return output;
	}

	static void FillRollingSum1D(Span<float> outValues, ReadOnlySpan<float> cumSpan, int length, int window)
	{
		int i = 0;
		int warmEnd = Math.Min(window, length);
		for (; i < warmEnd; i++)
			outValues[i] = cumSpan[i];

		for (; i < length; i++)
			outValues[i] = cumSpan[i] - cumSpan[i - window];
	}

	static void FillRollingSum1D(Span<int> outValues, ReadOnlySpan<int> cumSpan, int length, int window)
	{
		int i = 0;
		int warmEnd = Math.Min(window, length);
		for (; i < warmEnd; i++)
			outValues[i] = cumSpan[i];

		for (; i < length; i++)
			outValues[i] = cumSpan[i] - cumSpan[i - window];
	}

	static void FillRollingSum2D(Span<float> outValues, ReadOnlySpan<float> cumSpan, int length, int cols, int window)
	{
		for (int offset = 0; offset < length; offset += cols)
			FillRollingSum1D(outValues.Slice(offset, cols), cumSpan.Slice(offset, cols), cols, window);
	}

	static void FillRollingSum2D(Span<int> outValues, ReadOnlySpan<int> cumSpan, int length, int cols, int window)
	{
		for (int offset = 0; offset < length; offset += cols)
			FillRollingSum1D(outValues.Slice(offset, cols), cumSpan.Slice(offset, cols), cols, window);
	}

	static void FillMovingAverage1D(Span<float> outValues, ReadOnlySpan<float> cumSpan, int length, int window)
	{
		int i = 0;
		int warmEnd = Math.Min(window, length);
		for (; i < warmEnd; i++)
			outValues[i] = cumSpan[i] / (i + 1);

		for (; i < length; i++)
			outValues[i] = (cumSpan[i] - cumSpan[i - window]) / window;
	}

	static void FillMovingAverage2D(Span<float> outValues, ReadOnlySpan<float> cumSpan, int length, int cols, int window)
	{
		for (int offset = 0; offset < length; offset += cols)
			FillMovingAverage1D(outValues.Slice(offset, cols), cumSpan.Slice(offset, cols), cols, window);
	}

	static void FillAverageFromRolling1D(Span<float> outValues, ReadOnlySpan<int> rollingSpan, int length, int window)
	{
		int i = 0;
		int warmEnd = Math.Min(window, length);
		for (; i < warmEnd; i++)
			outValues[i] = rollingSpan[i] / (float)(i + 1);

		for (; i < length; i++)
			outValues[i] = rollingSpan[i] / (float)window;
	}

	static void FillAverageFromRolling2D(Span<float> outValues, ReadOnlySpan<int> rollingSpan, int length, int cols, int window)
	{
		for (int offset = 0; offset < length; offset += cols)
			FillAverageFromRolling1D(outValues.Slice(offset, cols), rollingSpan.Slice(offset, cols), cols, window);
	}
}
