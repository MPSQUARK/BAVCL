using System.Text;

namespace BAVCL.Core;

public partial class VectorBase<T>
{
	public string ToCSV()
	{
		SyncCPU();
		var stringBuilder = new StringBuilder();

		if (Columns > 1)
		{
			for (int i = 0; i < Length; i++)
				stringBuilder.Append($"{Value[i]},");

			return stringBuilder.ToString();
		}

		if (Columns == 0)
		{
			for (int i = 0; i < Length; i++)
				stringBuilder.Append($"{Value[i]},");

			return stringBuilder.ToString();
		}

		stringBuilder.Append($"{Value[0]},");

		for (int i = 1; i < Length; i++)
		{
			if (i % Columns == 0)
				stringBuilder.AppendLine();

			stringBuilder.Append($"{Value[i]},");
		}

		return stringBuilder.ToString();
	}
}
