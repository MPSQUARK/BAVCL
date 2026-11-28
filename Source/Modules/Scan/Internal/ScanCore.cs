namespace BAVCL.Modules.Scan;

internal static class ScanCore
{
	internal static Vector CumSumInclusive1D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumSumInclusiveIP1D(copy, upload);
		return copy;
	}

	internal static Vector CumSumInclusive2D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumSumInclusiveIP2D(copy, upload);
		return copy;
	}

	internal static VectorInt CumSumInclusive1D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumSumInclusiveIP1D(copy, upload);
		return copy;
	}

	internal static VectorInt CumSumInclusive2D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumSumInclusiveIP2D(copy, upload);
		return copy;
	}

	internal static Vector CumSumExclusive1D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumSumExclusiveIP1D(copy, upload);
		return copy;
	}

	internal static Vector CumSumExclusive2D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumSumExclusiveIP2D(copy, upload);
		return copy;
	}

	internal static VectorInt CumSumExclusive1D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumSumExclusiveIP1D(copy, upload);
		return copy;
	}

	internal static VectorInt CumSumExclusive2D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumSumExclusiveIP2D(copy, upload);
		return copy;
	}

	internal static Vector CumProdInclusive1D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumProdInclusiveIP1D(copy, upload);
		return copy;
	}

	internal static Vector CumProdInclusive2D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumProdInclusiveIP2D(copy, upload);
		return copy;
	}

	internal static VectorInt CumProdInclusive1D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumProdInclusiveIP1D(copy, upload);
		return copy;
	}

	internal static VectorInt CumProdInclusive2D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumProdInclusiveIP2D(copy, upload);
		return copy;
	}

	internal static Vector CumProdExclusive1D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumProdExclusiveIP1D(copy, upload);
		return copy;
	}

	internal static Vector CumProdExclusive2D(Vector vector, Upload upload = Upload.Now)
	{
		Vector copy = vector.Copy();
		CumProdExclusiveIP2D(copy, upload);
		return copy;
	}

	internal static VectorInt CumProdExclusive1D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumProdExclusiveIP1D(copy, upload);
		return copy;
	}

	internal static VectorInt CumProdExclusive2D(VectorInt vector, Upload upload = Upload.Now)
	{
		VectorInt copy = vector.Copy();
		CumProdExclusiveIP2D(copy, upload);
		return copy;
	}

	internal static void CumSumInclusiveIP1D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumInclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumSumInclusiveIP2D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumInclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}

	internal static void CumSumInclusiveIP1D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumInclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumSumInclusiveIP2D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumInclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}

	internal static void CumSumExclusiveIP1D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumExclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumSumExclusiveIP2D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumExclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}

	internal static void CumSumExclusiveIP1D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumExclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumSumExclusiveIP2D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.SumExclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}

	internal static void CumProdInclusiveIP1D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductInclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumProdInclusiveIP2D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductInclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}

	internal static void CumProdInclusiveIP1D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductInclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumProdInclusiveIP2D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductInclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}

	internal static void CumProdExclusiveIP1D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductExclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumProdExclusiveIP2D(Vector vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductExclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}

	internal static void CumProdExclusiveIP1D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductExclusive(vector.Value, offset: 0, vector.Length);
	}

	internal static void CumProdExclusiveIP2D(VectorInt vector, Upload upload = Upload.Now)
	{
		if (vector.Length == 0)
			return;

		using (vector.CpuScope(upload))
			ScanCpuSegment.ProductExclusive2D(vector.Value, vector.ElementsPerRow(), vector.RowCount());
	}
}
