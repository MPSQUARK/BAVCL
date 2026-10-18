using BAVCL.GpuAlgorithms;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Statistics;

internal static class GpuRollingStatsCore
{
	internal static Vector RollingSumX1D(Vector vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanAlgorithms.CumSumInclusiveX1D(vector);
		return LaunchRollingSumFloat1D(vector, cum, window);
	}

	internal static Vector RollingSumX2D(Vector vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanAlgorithms.CumSumInclusiveX2D(vector);
		return LaunchRollingSumFloat2D(vector, cum, window);
	}

	internal static VectorInt RollingSumX1D(VectorInt vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanAlgorithms.CumSumInclusiveX1D(vector);
		return LaunchRollingSumInt1D(vector, cum, window);
	}

	internal static VectorInt RollingSumX2D(VectorInt vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanAlgorithms.CumSumInclusiveX2D(vector);
		return LaunchRollingSumInt2D(vector, cum, window);
	}

	internal static Vector MovingAverageX1D(Vector vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanAlgorithms.CumSumInclusiveX1D(vector);
		return LaunchMovingAverageFloat1D(vector, cum, window);
	}

	internal static Vector MovingAverageX2D(Vector vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		Vector cum = ScanAlgorithms.CumSumInclusiveX2D(vector);
		return LaunchMovingAverageFloat2D(vector, cum, window);
	}

	internal static Vector MovingAverageX1D(VectorInt vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanAlgorithms.CumSumInclusiveX1D(vector);
		VectorInt rolling = LaunchRollingSumInt1D(vector, cum, window);
		return LaunchMovingAverageFromIntRolling1D(rolling, window);
	}

	internal static Vector MovingAverageX2D(VectorInt vector, int window)
	{
		Guard.RollingWindow(window, vector.Length);
		VectorInt cum = ScanAlgorithms.CumSumInclusiveX2D(vector);
		VectorInt rolling = LaunchRollingSumInt2D(vector, cum, window);
		return LaunchMovingAverageFromIntRolling2D(rolling, window);
	}

	static Vector LaunchRollingSumFloat1D(Vector vector, Vector cum, int window)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length, vector.Columns);
		int length = vector.Length;
		ArrayView1D<float, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<float, Stride1D.Dense> cumView = cum.GetBuffer().View;

		using (GpuScope.Begin(output, cum))
		{
			gpu.rollingSumFloat1D(gpu.DefaultStream, length, outputView, cumView, window);
			gpu.Synchronize();
		}

		return output;
	}

	static Vector LaunchRollingSumFloat2D(Vector vector, Vector cum, int window)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length, vector.Columns);
		int length = vector.Length;
		ArrayView1D<float, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<float, Stride1D.Dense> cumView = cum.GetBuffer().View;

		using (GpuScope.Begin(output, cum))
		{
			gpu.rollingSumFloat2D(gpu.DefaultStream, length, outputView, cumView, window, vector.Columns);
			gpu.Synchronize();
		}

		return output;
	}

	static VectorInt LaunchRollingSumInt1D(VectorInt vector, VectorInt cum, int window)
	{
		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, vector.Length, vector.Columns);
		int length = vector.Length;
		ArrayView1D<int, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<int, Stride1D.Dense> cumView = cum.GetBuffer().View;

		using (GpuScope.Begin(output, cum))
		{
			gpu.rollingSumInt1D(gpu.DefaultStream, length, outputView, cumView, window);
			gpu.Synchronize();
		}

		return output;
	}

	static VectorInt LaunchRollingSumInt2D(VectorInt vector, VectorInt cum, int window)
	{
		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, vector.Length, vector.Columns);
		int length = vector.Length;
		ArrayView1D<int, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<int, Stride1D.Dense> cumView = cum.GetBuffer().View;

		using (GpuScope.Begin(output, cum))
		{
			gpu.rollingSumInt2D(gpu.DefaultStream, length, outputView, cumView, window, vector.Columns);
			gpu.Synchronize();
		}

		return output;
	}

	static Vector LaunchMovingAverageFloat1D(Vector vector, Vector cum, int window)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length, vector.Columns);
		int length = vector.Length;
		ArrayView1D<float, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<float, Stride1D.Dense> cumView = cum.GetBuffer().View;

		using (GpuScope.Begin(output, cum))
		{
			gpu.rollingAverageFloat1D(gpu.DefaultStream, length, outputView, cumView, window);
			gpu.Synchronize();
		}

		return output;
	}

	static Vector LaunchMovingAverageFloat2D(Vector vector, Vector cum, int window)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length, vector.Columns);
		int length = vector.Length;
		ArrayView1D<float, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<float, Stride1D.Dense> cumView = cum.GetBuffer().View;

		using (GpuScope.Begin(output, cum))
		{
			gpu.rollingAverageFloat2D(gpu.DefaultStream, length, outputView, cumView, window, vector.Columns);
			gpu.Synchronize();
		}

		return output;
	}

	static Vector LaunchMovingAverageFromIntRolling1D(VectorInt rolling, int window)
	{
		GPU gpu = rolling.Gpu;
		Vector output = new(gpu, rolling.Length, rolling.Columns);
		int length = rolling.Length;
		ArrayView1D<float, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<int, Stride1D.Dense> rollingView = rolling.GetBuffer().View;

		using (GpuScope.Begin(output, rolling))
		{
			gpu.rollingAverageFromInt1D(gpu.DefaultStream, length, outputView, rollingView, window);
			gpu.Synchronize();
		}

		return output;
	}

	static Vector LaunchMovingAverageFromIntRolling2D(VectorInt rolling, int window)
	{
		GPU gpu = rolling.Gpu;
		Vector output = new(gpu, rolling.Length, rolling.Columns);
		int length = rolling.Length;
		ArrayView1D<float, Stride1D.Dense> outputView = output.GetBuffer().View;
		ArrayView1D<int, Stride1D.Dense> rollingView = rolling.GetBuffer().View;

		using (GpuScope.Begin(output, rolling))
		{
			gpu.rollingAverageFromInt2D(gpu.DefaultStream, length, outputView, rollingView, window, rolling.Columns);
			gpu.Synchronize();
		}

		return output;
	}
}
