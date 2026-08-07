namespace BAVCL.Modules.Sorting;

public static class VectorSorting
{
	extension(VectorInt)
	{
		public static VectorInt Sort(VectorInt vector, SortOrder order, bool syncToGpu = true) => SortCore.Sort(vector, order, syncToGpu);

		public static VectorInt SortAsc(VectorInt vector, bool syncToGpu = true) => SortCore.SortAsc(vector, syncToGpu);

		public static VectorInt SortDesc(VectorInt vector, bool syncToGpu = true) => SortCore.SortDesc(vector, syncToGpu);

		public static void SortIP(VectorInt vector, SortOrder order, bool syncToGpu = true) => SortCore.SortIP(vector, order, syncToGpu);

		public static void SortAscIP(VectorInt vector, bool syncToGpu = true) => SortCore.SortAscIP(vector, syncToGpu);

		public static void SortDescIP(VectorInt vector, bool syncToGpu = true) => SortCore.SortDescIP(vector, syncToGpu);

		public static VectorInt SortX(VectorInt vector, SortOrder order) => GpuSortingCore.SortX(vector, order);

		public static VectorInt SortAscX(VectorInt vector) => GpuSortingCore.SortAscX(vector);

		public static VectorInt SortDescX(VectorInt vector) => GpuSortingCore.SortDescX(vector);

		public static void SortXIP(VectorInt vector, SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public static void SortAscXIP(VectorInt vector) => GpuSortingCore.SortAscXIP(vector);

		public static void SortDescXIP(VectorInt vector) => GpuSortingCore.SortDescXIP(vector);

		public static VectorInt Argsort(VectorInt vector, SortOrder order, bool syncToGpu = true) => ArgsortCore.Argsort(vector, order, syncToGpu);

		public static VectorInt ArgsortAsc(VectorInt vector, bool syncToGpu = true) => ArgsortCore.ArgsortAsc(vector, syncToGpu);

		public static VectorInt ArgsortDesc(VectorInt vector, bool syncToGpu = true) => ArgsortCore.ArgsortDesc(vector, syncToGpu);

		public static void ArgsortIP(VectorInt vector, VectorInt indices, SortOrder order, bool syncToGpu = true) =>
			ArgsortCore.ArgsortIP(vector, indices, order, syncToGpu);

		public static void ArgsortAscIP(VectorInt vector, VectorInt indices, bool syncToGpu = true) =>
			ArgsortCore.ArgsortAscIP(vector, indices, syncToGpu);

		public static void ArgsortDescIP(VectorInt vector, VectorInt indices, bool syncToGpu = true) =>
			ArgsortCore.ArgsortDescIP(vector, indices, syncToGpu);

		public static VectorInt ArgsortX(VectorInt vector, SortOrder order) => GpuSortingCore.ArgsortX(vector, order);

		public static VectorInt ArgsortAscX(VectorInt vector) => GpuSortingCore.ArgsortAscX(vector);

		public static VectorInt ArgsortDescX(VectorInt vector) => GpuSortingCore.ArgsortDescX(vector);

		public static void ArgsortXIP(VectorInt vector, VectorInt indices, SortOrder order) =>
			GpuSortingCore.ArgsortXIP(vector, indices, order);

		public static void ArgsortAscXIP(VectorInt vector, VectorInt indices) => GpuSortingCore.ArgsortAscXIP(vector, indices);

		public static void ArgsortDescXIP(VectorInt vector, VectorInt indices) => GpuSortingCore.ArgsortDescXIP(vector, indices);
	}

	extension(Vector)
	{
		public static Vector Sort(Vector vector, SortOrder order, bool syncToGpu = true) => SortCore.Sort(vector, order, syncToGpu);

		public static Vector SortAsc(Vector vector, bool syncToGpu = true) => SortCore.SortAsc(vector, syncToGpu);

		public static Vector SortDesc(Vector vector, bool syncToGpu = true) => SortCore.SortDesc(vector, syncToGpu);

		public static void SortIP(Vector vector, SortOrder order, bool syncToGpu = true) => SortCore.SortIP(vector, order, syncToGpu);

		public static void SortAscIP(Vector vector, bool syncToGpu = true) => SortCore.SortAscIP(vector, syncToGpu);

		public static void SortDescIP(Vector vector, bool syncToGpu = true) => SortCore.SortDescIP(vector, syncToGpu);

		public static Vector SortX(Vector vector, SortOrder order) => GpuSortingCore.SortX(vector, order);

		public static Vector SortAscX(Vector vector) => GpuSortingCore.SortAscX(vector);

		public static Vector SortDescX(Vector vector) => GpuSortingCore.SortDescX(vector);

		public static void SortXIP(Vector vector, SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public static void SortAscXIP(Vector vector) => GpuSortingCore.SortAscXIP(vector);

		public static void SortDescXIP(Vector vector) => GpuSortingCore.SortDescXIP(vector);

		public static VectorInt Argsort(Vector vector, SortOrder order, bool syncToGpu = true) => ArgsortCore.Argsort(vector, order, syncToGpu);

		public static VectorInt ArgsortAsc(Vector vector, bool syncToGpu = true) => ArgsortCore.ArgsortAsc(vector, syncToGpu);

		public static VectorInt ArgsortDesc(Vector vector, bool syncToGpu = true) => ArgsortCore.ArgsortDesc(vector, syncToGpu);

		public static void ArgsortIP(Vector vector, VectorInt indices, SortOrder order, bool syncToGpu = true) =>
			ArgsortCore.ArgsortIP(vector, indices, order, syncToGpu);

		public static void ArgsortAscIP(Vector vector, VectorInt indices, bool syncToGpu = true) =>
			ArgsortCore.ArgsortAscIP(vector, indices, syncToGpu);

		public static void ArgsortDescIP(Vector vector, VectorInt indices, bool syncToGpu = true) =>
			ArgsortCore.ArgsortDescIP(vector, indices, syncToGpu);

		public static VectorInt ArgsortX(Vector vector, SortOrder order) => GpuSortingCore.ArgsortX(vector, order);

		public static VectorInt ArgsortAscX(Vector vector) => GpuSortingCore.ArgsortAscX(vector);

		public static VectorInt ArgsortDescX(Vector vector) => GpuSortingCore.ArgsortDescX(vector);

		public static void ArgsortXIP(Vector vector, VectorInt indices, SortOrder order) =>
			GpuSortingCore.ArgsortXIP(vector, indices, order);

		public static void ArgsortAscXIP(Vector vector, VectorInt indices) => GpuSortingCore.ArgsortAscXIP(vector, indices);

		public static void ArgsortDescXIP(Vector vector, VectorInt indices) => GpuSortingCore.ArgsortDescXIP(vector, indices);
	}
}

public static class SortingModule
{
	extension(VectorInt vector)
	{
		public VectorInt Sort(SortOrder order, bool syncToGpu = true) => SortCore.Sort(vector, order, syncToGpu);

		public VectorInt SortAsc(bool syncToGpu = true) => SortCore.SortAsc(vector, syncToGpu);

		public VectorInt SortDesc(bool syncToGpu = true) => SortCore.SortDesc(vector, syncToGpu);

		public void SortIP(SortOrder order, bool syncToGpu = true) => SortCore.SortIP(vector, order, syncToGpu);

		public void SortAscIP(bool syncToGpu = true) => SortCore.SortAscIP(vector, syncToGpu);

		public void SortDescIP(bool syncToGpu = true) => SortCore.SortDescIP(vector, syncToGpu);

		public VectorInt SortX(SortOrder order) => GpuSortingCore.SortX(vector, order);

		public VectorInt SortAscX() => GpuSortingCore.SortAscX(vector);

		public VectorInt SortDescX() => GpuSortingCore.SortDescX(vector);

		public void SortXIP(SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public void SortAscXIP() => GpuSortingCore.SortAscXIP(vector);

		public void SortDescXIP() => GpuSortingCore.SortDescXIP(vector);

		public VectorInt Argsort(SortOrder order, bool syncToGpu = true) => ArgsortCore.Argsort(vector, order, syncToGpu);

		public VectorInt ArgsortAsc(bool syncToGpu = true) => ArgsortCore.ArgsortAsc(vector, syncToGpu);

		public VectorInt ArgsortDesc(bool syncToGpu = true) => ArgsortCore.ArgsortDesc(vector, syncToGpu);

		public void ArgsortIP(VectorInt indices, SortOrder order, bool syncToGpu = true) =>
			ArgsortCore.ArgsortIP(vector, indices, order, syncToGpu);

		public void ArgsortAscIP(VectorInt indices, bool syncToGpu = true) => ArgsortCore.ArgsortAscIP(vector, indices, syncToGpu);

		public void ArgsortDescIP(VectorInt indices, bool syncToGpu = true) => ArgsortCore.ArgsortDescIP(vector, indices, syncToGpu);

		public VectorInt ArgsortX(SortOrder order) => GpuSortingCore.ArgsortX(vector, order);

		public VectorInt ArgsortAscX() => GpuSortingCore.ArgsortAscX(vector);

		public VectorInt ArgsortDescX() => GpuSortingCore.ArgsortDescX(vector);

		public void ArgsortXIP(VectorInt indices, SortOrder order) => GpuSortingCore.ArgsortXIP(vector, indices, order);

		public void ArgsortAscXIP(VectorInt indices) => GpuSortingCore.ArgsortAscXIP(vector, indices);

		public void ArgsortDescXIP(VectorInt indices) => GpuSortingCore.ArgsortDescXIP(vector, indices);
	}

	extension(Vector vector)
	{
		public Vector Sort(SortOrder order, bool syncToGpu = true) => SortCore.Sort(vector, order, syncToGpu);

		public Vector SortAsc(bool syncToGpu = true) => SortCore.SortAsc(vector, syncToGpu);

		public Vector SortDesc(bool syncToGpu = true) => SortCore.SortDesc(vector, syncToGpu);

		public void SortIP(SortOrder order, bool syncToGpu = true) => SortCore.SortIP(vector, order, syncToGpu);

		public void SortAscIP(bool syncToGpu = true) => SortCore.SortAscIP(vector, syncToGpu);

		public void SortDescIP(bool syncToGpu = true) => SortCore.SortDescIP(vector, syncToGpu);

		public Vector SortX(SortOrder order) => GpuSortingCore.SortX(vector, order);

		public Vector SortAscX() => GpuSortingCore.SortAscX(vector);

		public Vector SortDescX() => GpuSortingCore.SortDescX(vector);

		public void SortXIP(SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public void SortAscXIP() => GpuSortingCore.SortAscXIP(vector);

		public void SortDescXIP() => GpuSortingCore.SortDescXIP(vector);

		public VectorInt Argsort(SortOrder order, bool syncToGpu = true) => ArgsortCore.Argsort(vector, order, syncToGpu);

		public VectorInt ArgsortAsc(bool syncToGpu = true) => ArgsortCore.ArgsortAsc(vector, syncToGpu);

		public VectorInt ArgsortDesc(bool syncToGpu = true) => ArgsortCore.ArgsortDesc(vector, syncToGpu);

		public void ArgsortIP(VectorInt indices, SortOrder order, bool syncToGpu = true) =>
			ArgsortCore.ArgsortIP(vector, indices, order, syncToGpu);

		public void ArgsortAscIP(VectorInt indices, bool syncToGpu = true) => ArgsortCore.ArgsortAscIP(vector, indices, syncToGpu);

		public void ArgsortDescIP(VectorInt indices, bool syncToGpu = true) => ArgsortCore.ArgsortDescIP(vector, indices, syncToGpu);

		public VectorInt ArgsortX(SortOrder order) => GpuSortingCore.ArgsortX(vector, order);

		public VectorInt ArgsortAscX() => GpuSortingCore.ArgsortAscX(vector);

		public VectorInt ArgsortDescX() => GpuSortingCore.ArgsortDescX(vector);

		public void ArgsortXIP(VectorInt indices, SortOrder order) => GpuSortingCore.ArgsortXIP(vector, indices, order);

		public void ArgsortAscXIP(VectorInt indices) => GpuSortingCore.ArgsortAscXIP(vector, indices);

		public void ArgsortDescXIP(VectorInt indices) => GpuSortingCore.ArgsortDescXIP(vector, indices);
	}
}
