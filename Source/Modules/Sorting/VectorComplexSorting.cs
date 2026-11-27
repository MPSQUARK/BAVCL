using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BAVCL.Core;
using BAVCL.Core.Exceptions;
using BAVCL.Core.Memory;
using BAVCL.Types;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Sorting;

/// <summary>CPU sorting and argsorting for complex vectors.</summary>
public static class VectorComplexSorting
{
	extension(VectorComplex64 vector)
	{
		public VectorComplex64 Sort(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			SortCore(vector, mode, order);

		public void SortIP(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			SortInPlace(vector, mode, order);

		public VectorInt Argsort(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			ArgsortCore(vector, mode, order);

		public VectorComplex64 SortX(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			SortGpu(vector, mode, order);

		public void SortXIP(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			SortGpuInPlace(vector, mode, order);

		public VectorInt ArgsortX(ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			ArgsortGpu(vector, mode, order);

		public void ArgsortXIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude, SortOrder order = SortOrder.Ascending) =>
			ArgsortGpuInPlace(vector, indices, mode, order);

		public VectorComplex64 SortAscX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortAscX(vector, mode);

		public VectorComplex64 SortDescX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortDescX(vector, mode);

		public void SortAscXIP(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortAscXIP(vector, mode);

		public void SortDescXIP(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.SortDescXIP(vector, mode);

		public VectorInt ArgsortAscX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortAscX(vector, mode);

		public VectorInt ArgsortDescX(ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortDescX(vector, mode);

		public void ArgsortAscXIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortAscXIP(vector, indices, mode);

		public void ArgsortDescXIP(VectorInt indices, ComplexSortMode mode = ComplexSortMode.Magnitude) =>
			ComplexSortingCore.ArgsortDescXIP(vector, indices, mode);
	}

	static VectorComplex64 SortCore(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		Complex[] values = vector.RetrieveReadOnlySpan().ToArray();
		SortRows(values, vector.Columns, mode, order);
		return new VectorComplex64(vector.Gpu, values, vector.Columns);
	}

	static void SortInPlace(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		using (var scope = vector.CpuScope())
		{
			Complex[] values = vector.Value;
			SortRows(values, vector.Columns, mode, order);
			EditableView<Complex> view = scope.View;
			for (int i = 0; i < values.Length; i++)
				view[i] = values[i];
		}
	}

	static VectorInt ArgsortCore(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		Complex[] values = vector.RetrieveReadOnlySpan().ToArray();
		int[] indices = new int[values.Length];
		for (int row = 0; row < RowCount(vector.Length, vector.Columns); row++)
		{
			int offset = vector.Is1D() ? 0 : row * vector.Columns;
			int count = vector.Is1D() ? values.Length : vector.Columns;
			for (int i = 0; i < count; i++)
				indices[offset + i] = i;
			Array.Sort(indices, offset, count, Comparer<int>.Create((left, right) =>
				Compare(values[offset + left], values[offset + right], mode, order)));
		}
		return new VectorInt(vector.Gpu, indices, vector.Columns);
	}

	static void SortRows(Complex[] values, int columns, ComplexSortMode mode, SortOrder order)
	{
		if (values.Length < 2)
			return;
		if (columns <= 1)
		{
			Array.Sort(values, Comparer<Complex>.Create((left, right) => Compare(left, right, mode, order)));
			return;
		}
		for (int row = 0; row < values.Length / columns; row++)
			Array.Sort(values, row * columns, columns, Comparer<Complex>.Create((left, right) => Compare(left, right, mode, order)));
	}

	static int Compare(Complex left, Complex right, ComplexSortMode mode, SortOrder order)
	{
		int result = mode switch
		{
			ComplexSortMode.Magnitude => CompareDouble(left.Magnitude, right.Magnitude),
			ComplexSortMode.Complex => CompareComponents(left, right),
			ComplexSortMode.Real => CompareDouble(left.Real, right.Real),
			ComplexSortMode.Imaginary => CompareDouble(left.Imaginary, right.Imaginary),
			_ => throw new ArgumentOutOfRangeException(nameof(mode)),
		};
		if (result == 0 && mode == ComplexSortMode.Magnitude)
			result = CompareComponents(left, right);
		if (result == 0 && mode == ComplexSortMode.Real)
			result = CompareDouble(left.Imaginary, right.Imaginary);
		if (result == 0 && mode == ComplexSortMode.Imaginary)
			result = CompareDouble(left.Real, right.Real);
		return order == SortOrder.Descending ? -result : result;
	}

	static int CompareComponents(Complex left, Complex right)
	{
		int real = CompareDouble(left.Real, right.Real);
		return real != 0 ? real : CompareDouble(left.Imaginary, right.Imaginary);
	}

	static int CompareDouble(double left, double right)
	{
		if (double.IsNaN(left))
			return double.IsNaN(right) ? 0 : 1;
		if (double.IsNaN(right))
			return -1;
		int result = left.CompareTo(right);
		if (result != 0)
			return result;
		return BitConverter.DoubleToInt64Bits(left).CompareTo(BitConverter.DoubleToInt64Bits(right));
	}

	static int RowCount(int length, int columns) => columns <= 1 ? 1 : length / columns;

	static VectorComplex64 SortGpu(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		Complex[] values = RunGpuSort(vector, mode, order, withIndices: false, out _);
		return new VectorComplex64(vector.Gpu, values, vector.Columns);
	}

	static void SortGpuInPlace(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		Complex[] values = RunGpuSort(vector, mode, order, withIndices: false, out _);
		using var scope = vector.CpuScope();
		EditableView<Complex> view = scope.View;
		for (int i = 0; i < values.Length; i++)
			view[i] = values[i];
	}

	static VectorInt ArgsortGpu(VectorComplex64 vector, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		_ = RunGpuSort(vector, mode, order, withIndices: true, out int[] indices);
		return new VectorInt(vector.Gpu, indices, vector.Columns);
	}

	static void ArgsortGpuInPlace(VectorComplex64 vector, VectorInt indices, ComplexSortMode mode, SortOrder order)
	{
		ValidateMode(mode);
		if (indices.Length != vector.Length || indices.Columns != vector.Columns)
			throw new ShapeMismatchException(vector.Shape(), indices.Shape());
		_ = RunGpuSort(vector, mode, order, withIndices: true, out int[] values);
		using var scope = indices.CpuScope();
		EditableView<int> view = scope.View;
		for (int i = 0; i < values.Length; i++)
			view[i] = values[i];
	}

	static Complex[] RunGpuSort(
		VectorComplex64 vector,
		ComplexSortMode mode,
		SortOrder order,
		bool withIndices,
		out int[] indices)
	{
		ValidateMode(mode);
		if (vector.Length == 0)
		{
			indices = [];
			return [];
		}
		KernelModuleLoader.Load<Complex>(vector.Gpu, KernelDomain.Sorting);
		Complex64Device[] source = ToDevice(vector.RetrieveReadOnlySpan());
		indices = new int[source.Length];
		for (int row = 0; row < RowCount(vector.Length, vector.Columns); row++)
		{
			int offset = vector.Is1D() ? 0 : row * vector.Columns;
			int count = vector.Is1D() ? vector.Length : vector.Columns;
			for (int i = 0; i < count; i++)
				indices[offset + i] = i;
		}
		using BufferEntity<Complex64Device> values = BufferPools.For(vector.Gpu).Complex64.Rent(source.Length);
		using BufferEntity<int> indexBuffer = BufferPools.For(vector.Gpu).Int.Rent(Math.Max(source.Length, 1));
		values.CopyFromCPU(source);
		if (withIndices)
			indexBuffer.CopyFromCPU(indices);

		using (GpuScope.Begin(values.PinTarget, withIndices ? [indexBuffer.PinTarget] : []))
		{
			for (int row = 0; row < RowCount(vector.Length, vector.Columns); row++)
			{
				int offset = vector.Is1D() ? 0 : row * vector.Columns;
				int count = vector.Is1D() ? vector.Length : vector.Columns;
				for (int phase = 0; phase < count; phase++)
				{
					vector.Gpu.complexSortPhase(
						vector.Gpu.DefaultStream,
						Math.Max((count - (phase & 1)) / 2, 0),
						values.View,
						withIndices ? indexBuffer.View : default,
						offset,
						count,
						phase,
						new SpecializedValue<int>((int)mode),
						new SpecializedValue<int>(order == SortOrder.Descending ? 1 : 0));
					vector.Gpu.Synchronize();
				}
			}
			values.CopyToCPU(source);
			if (withIndices)
				indexBuffer.CopyToCPU(indices);
		}
		return FromDevice(source);
	}

	static Complex64Device[] ToDevice(ReadOnlySpan<Complex> values)
	{
		Complex64Device[] result = new Complex64Device[values.Length];
		for (int i = 0; i < values.Length; i++)
			result[i] = new Complex64Device(values[i].Real, values[i].Imaginary);
		return result;
	}

	static Complex[] FromDevice(Complex64Device[] values)
	{
		Complex[] result = new Complex[values.Length];
		for (int i = 0; i < values.Length; i++)
			result[i] = new Complex(values[i].Real, values[i].Imaginary);
		return result;
	}

	static void ValidateMode(ComplexSortMode mode)
	{
		if (mode is not (ComplexSortMode.Magnitude or ComplexSortMode.Complex or
			ComplexSortMode.Real or ComplexSortMode.Imaginary))
			throw new ArgumentOutOfRangeException(nameof(mode));
	}
}
