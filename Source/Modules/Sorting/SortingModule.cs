namespace BAVCL.Modules.Sorting;

public static class VectorSorting
{
	extension(VectorInt)
	{
		public static VectorInt Sort(VectorInt vector, SortOrder order, Upload upload = Upload.Now) => SortCore.Sort(vector, order, upload);

		public static VectorInt SortAsc(VectorInt vector, Upload upload = Upload.Now) => SortCore.SortAsc(vector, upload);

		public static VectorInt SortDesc(VectorInt vector, Upload upload = Upload.Now) => SortCore.SortDesc(vector, upload);

		public static void SortIP(VectorInt vector, SortOrder order, Upload upload = Upload.Now) => SortCore.SortIP(vector, order, upload);

		public static void SortAscIP(VectorInt vector, Upload upload = Upload.Now) => SortCore.SortAscIP(vector, upload);

		public static void SortDescIP(VectorInt vector, Upload upload = Upload.Now) => SortCore.SortDescIP(vector, upload);

		public static VectorInt SortX(VectorInt vector, SortOrder order) => GpuSortingCore.SortX(vector, order);

		public static VectorInt SortAscX(VectorInt vector) => GpuSortingCore.SortAscX(vector);

		public static VectorInt SortDescX(VectorInt vector) => GpuSortingCore.SortDescX(vector);

		public static void SortXIP(VectorInt vector, SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public static void SortAscXIP(VectorInt vector) => GpuSortingCore.SortAscXIP(vector);

		public static void SortDescXIP(VectorInt vector) => GpuSortingCore.SortDescXIP(vector);

		public static VectorInt Argsort(VectorInt vector, SortOrder order, Upload upload = Upload.Now) => ArgsortCore.Argsort(vector, order, upload);

		public static VectorInt ArgsortAsc(VectorInt vector, Upload upload = Upload.Now) => ArgsortCore.ArgsortAsc(vector, upload);

		public static VectorInt ArgsortDesc(VectorInt vector, Upload upload = Upload.Now) => ArgsortCore.ArgsortDesc(vector, upload);

		public static void ArgsortIP(VectorInt vector, VectorInt indices, SortOrder order, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortIP(vector, indices, order, upload);

		public static void ArgsortAscIP(VectorInt vector, VectorInt indices, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortAscIP(vector, indices, upload);

		public static void ArgsortDescIP(VectorInt vector, VectorInt indices, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortDescIP(vector, indices, upload);

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
		public static Vector Sort(Vector vector, SortOrder order, Upload upload = Upload.Now) => SortCore.Sort(vector, order, upload);

		public static Vector SortAsc(Vector vector, Upload upload = Upload.Now) => SortCore.SortAsc(vector, upload);

		public static Vector SortDesc(Vector vector, Upload upload = Upload.Now) => SortCore.SortDesc(vector, upload);

		public static void SortIP(Vector vector, SortOrder order, Upload upload = Upload.Now) => SortCore.SortIP(vector, order, upload);

		public static void SortAscIP(Vector vector, Upload upload = Upload.Now) => SortCore.SortAscIP(vector, upload);

		public static void SortDescIP(Vector vector, Upload upload = Upload.Now) => SortCore.SortDescIP(vector, upload);

		public static Vector SortX(Vector vector, SortOrder order) => GpuSortingCore.SortX(vector, order);

		public static Vector SortAscX(Vector vector) => GpuSortingCore.SortAscX(vector);

		public static Vector SortDescX(Vector vector) => GpuSortingCore.SortDescX(vector);

		public static void SortXIP(Vector vector, SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public static void SortAscXIP(Vector vector) => GpuSortingCore.SortAscXIP(vector);

		public static void SortDescXIP(Vector vector) => GpuSortingCore.SortDescXIP(vector);

		public static VectorInt Argsort(Vector vector, SortOrder order, Upload upload = Upload.Now) => ArgsortCore.Argsort(vector, order, upload);

		public static VectorInt ArgsortAsc(Vector vector, Upload upload = Upload.Now) => ArgsortCore.ArgsortAsc(vector, upload);

		public static VectorInt ArgsortDesc(Vector vector, Upload upload = Upload.Now) => ArgsortCore.ArgsortDesc(vector, upload);

		public static void ArgsortIP(Vector vector, VectorInt indices, SortOrder order, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortIP(vector, indices, order, upload);

		public static void ArgsortAscIP(Vector vector, VectorInt indices, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortAscIP(vector, indices, upload);

		public static void ArgsortDescIP(Vector vector, VectorInt indices, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortDescIP(vector, indices, upload);

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
		public VectorInt Sort(SortOrder order, Upload upload = Upload.Now) => SortCore.Sort(vector, order, upload);

		public VectorInt SortAsc(Upload upload = Upload.Now) => SortCore.SortAsc(vector, upload);

		public VectorInt SortDesc(Upload upload = Upload.Now) => SortCore.SortDesc(vector, upload);

		public void SortIP(SortOrder order, Upload upload = Upload.Now) => SortCore.SortIP(vector, order, upload);

		public void SortAscIP(Upload upload = Upload.Now) => SortCore.SortAscIP(vector, upload);

		public void SortDescIP(Upload upload = Upload.Now) => SortCore.SortDescIP(vector, upload);

		public VectorInt SortX(SortOrder order) => GpuSortingCore.SortX(vector, order);

		public VectorInt SortAscX() => GpuSortingCore.SortAscX(vector);

		public VectorInt SortDescX() => GpuSortingCore.SortDescX(vector);

		public void SortXIP(SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public void SortAscXIP() => GpuSortingCore.SortAscXIP(vector);

		public void SortDescXIP() => GpuSortingCore.SortDescXIP(vector);

		public VectorInt Argsort(SortOrder order, Upload upload = Upload.Now) => ArgsortCore.Argsort(vector, order, upload);

		public VectorInt ArgsortAsc(Upload upload = Upload.Now) => ArgsortCore.ArgsortAsc(vector, upload);

		public VectorInt ArgsortDesc(Upload upload = Upload.Now) => ArgsortCore.ArgsortDesc(vector, upload);

		public void ArgsortIP(VectorInt indices, SortOrder order, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortIP(vector, indices, order, upload);

		public void ArgsortAscIP(VectorInt indices, Upload upload = Upload.Now) => ArgsortCore.ArgsortAscIP(vector, indices, upload);

		public void ArgsortDescIP(VectorInt indices, Upload upload = Upload.Now) => ArgsortCore.ArgsortDescIP(vector, indices, upload);

		public VectorInt ArgsortX(SortOrder order) => GpuSortingCore.ArgsortX(vector, order);

		public VectorInt ArgsortAscX() => GpuSortingCore.ArgsortAscX(vector);

		public VectorInt ArgsortDescX() => GpuSortingCore.ArgsortDescX(vector);

		public void ArgsortXIP(VectorInt indices, SortOrder order) => GpuSortingCore.ArgsortXIP(vector, indices, order);

		public void ArgsortAscXIP(VectorInt indices) => GpuSortingCore.ArgsortAscXIP(vector, indices);

		public void ArgsortDescXIP(VectorInt indices) => GpuSortingCore.ArgsortDescXIP(vector, indices);
	}

	extension(Vector vector)
	{
		public Vector Sort(SortOrder order, Upload upload = Upload.Now) => SortCore.Sort(vector, order, upload);

		public Vector SortAsc(Upload upload = Upload.Now) => SortCore.SortAsc(vector, upload);

		public Vector SortDesc(Upload upload = Upload.Now) => SortCore.SortDesc(vector, upload);

		public void SortIP(SortOrder order, Upload upload = Upload.Now) => SortCore.SortIP(vector, order, upload);

		public void SortAscIP(Upload upload = Upload.Now) => SortCore.SortAscIP(vector, upload);

		public void SortDescIP(Upload upload = Upload.Now) => SortCore.SortDescIP(vector, upload);

		public Vector SortX(SortOrder order) => GpuSortingCore.SortX(vector, order);

		public Vector SortAscX() => GpuSortingCore.SortAscX(vector);

		public Vector SortDescX() => GpuSortingCore.SortDescX(vector);

		public void SortXIP(SortOrder order) => GpuSortingCore.SortXIP(vector, order);

		public void SortAscXIP() => GpuSortingCore.SortAscXIP(vector);

		public void SortDescXIP() => GpuSortingCore.SortDescXIP(vector);

		public VectorInt Argsort(SortOrder order, Upload upload = Upload.Now) => ArgsortCore.Argsort(vector, order, upload);

		public VectorInt ArgsortAsc(Upload upload = Upload.Now) => ArgsortCore.ArgsortAsc(vector, upload);

		public VectorInt ArgsortDesc(Upload upload = Upload.Now) => ArgsortCore.ArgsortDesc(vector, upload);

		public void ArgsortIP(VectorInt indices, SortOrder order, Upload upload = Upload.Now) =>
			ArgsortCore.ArgsortIP(vector, indices, order, upload);

		public void ArgsortAscIP(VectorInt indices, Upload upload = Upload.Now) => ArgsortCore.ArgsortAscIP(vector, indices, upload);

		public void ArgsortDescIP(VectorInt indices, Upload upload = Upload.Now) => ArgsortCore.ArgsortDescIP(vector, indices, upload);

		public VectorInt ArgsortX(SortOrder order) => GpuSortingCore.ArgsortX(vector, order);

		public VectorInt ArgsortAscX() => GpuSortingCore.ArgsortAscX(vector);

		public VectorInt ArgsortDescX() => GpuSortingCore.ArgsortDescX(vector);

		public void ArgsortXIP(VectorInt indices, SortOrder order) => GpuSortingCore.ArgsortXIP(vector, indices, order);

		public void ArgsortAscXIP(VectorInt indices) => GpuSortingCore.ArgsortAscXIP(vector, indices);

		public void ArgsortDescXIP(VectorInt indices) => GpuSortingCore.ArgsortDescXIP(vector, indices);
	}
}
