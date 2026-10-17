namespace BAVCL.Modules.Scan;

internal static class ScanCpuSegment
{
	internal static void SumInclusive(float[] values, int offset, int length) =>
		InclusiveAdd(values, offset, length);

	internal static void SumExclusive(float[] values, int offset, int length) =>
		ExclusiveAdd(values, offset, length);

	internal static void ProductInclusive(float[] values, int offset, int length) =>
		InclusiveMul(values, offset, length);

	internal static void ProductExclusive(float[] values, int offset, int length) =>
		ExclusiveMul(values, offset, length);

	internal static void SumInclusive(int[] values, int offset, int length) =>
		InclusiveAdd(values, offset, length);

	internal static void SumExclusive(int[] values, int offset, int length) =>
		ExclusiveAdd(values, offset, length);

	internal static void ProductInclusive(int[] values, int offset, int length) =>
		InclusiveMul(values, offset, length);

	internal static void ProductExclusive(int[] values, int offset, int length) =>
		ExclusiveMul(values, offset, length);

	internal static void SumInclusive2D(float[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			SumInclusive(values, offset, cols);
	}

	internal static void SumExclusive2D(float[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			SumExclusive(values, offset, cols);
	}

	internal static void ProductInclusive2D(float[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			ProductInclusive(values, offset, cols);
	}

	internal static void ProductExclusive2D(float[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			ProductExclusive(values, offset, cols);
	}

	internal static void SumInclusive2D(int[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			SumInclusive(values, offset, cols);
	}

	internal static void SumExclusive2D(int[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			SumExclusive(values, offset, cols);
	}

	internal static void ProductInclusive2D(int[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			ProductInclusive(values, offset, cols);
	}

	internal static void ProductExclusive2D(int[] values, int cols, int rowCount)
	{
		for (int offset = 0, row = 0; row < rowCount; row++, offset += cols)
			ProductExclusive(values, offset, cols);
	}

	static void InclusiveAdd(float[] values, int offset, int length)
	{
		if (length == 0)
			return;

		float acc = 0f;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			acc += values[index];
			values[index] = acc;
		}
	}

	static void ExclusiveAdd(float[] values, int offset, int length)
	{
		if (length == 0)
			return;

		float acc = 0f;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			float current = values[index];
			values[index] = acc;
			acc += current;
		}
	}

	static void InclusiveMul(float[] values, int offset, int length)
	{
		if (length == 0)
			return;

		float acc = 1f;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			acc *= values[index];
			values[index] = acc;
		}
	}

	static void ExclusiveMul(float[] values, int offset, int length)
	{
		if (length == 0)
			return;

		float acc = 1f;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			float current = values[index];
			values[index] = acc;
			acc *= current;
		}
	}

	static void InclusiveAdd(int[] values, int offset, int length)
	{
		if (length == 0)
			return;

		int acc = 0;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			acc += values[index];
			values[index] = acc;
		}
	}

	static void ExclusiveAdd(int[] values, int offset, int length)
	{
		if (length == 0)
			return;

		int acc = 0;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			int current = values[index];
			values[index] = acc;
			acc += current;
		}
	}

	static void InclusiveMul(int[] values, int offset, int length)
	{
		if (length == 0)
			return;

		int acc = 1;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			acc = unchecked(acc * values[index]);
			values[index] = acc;
		}
	}

	static void ExclusiveMul(int[] values, int offset, int length)
	{
		if (length == 0)
			return;

		int acc = 1;
		for (int i = 0; i < length; i++)
		{
			int index = offset + i;
			int current = values[index];
			values[index] = acc;
			acc = unchecked(acc * current);
		}
	}
}
