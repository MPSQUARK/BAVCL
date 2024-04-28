using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	public T Sum()
	{
		return Sum(this.Values);
	}

	public T Sum(T[] array)
	{
		return default;
	}

	public float Sum(float[] array)
	{
		SyncCPU();

		int vectorSize = System.Numerics.Vector<float>.Count;
		int i = 0;

		System.Numerics.Vector<float> sumVector = System.Numerics.Vector<float>.Zero;

		if (array.Length >= 1e4f)
		{
			System.Numerics.Vector<float> c = System.Numerics.Vector<float>.Zero;
			for (; i <= array.Length - vectorSize; i += vectorSize)
			{

				System.Numerics.Vector<float> input = new(array, i);

				System.Numerics.Vector<float> y = input - c;

				System.Numerics.Vector<float> t = sumVector + y;

				c = (t - sumVector) - y;

				sumVector = t;
			}
		}
		else
		{
			for (; i <= array.Length - vectorSize; i += vectorSize)
			{
				System.Numerics.Vector<float> vector = new(array, i);
				System.Numerics.Vector<float> v = vector;

				sumVector = System.Numerics.Vector.Add(sumVector, v);
			}
		}

		float result = 0;
		for (int j = 0; j < vectorSize; j++)
		{
			result += sumVector[j];
		}

		for (; i < array.Length; i++)
		{
			result += array[i];
		}
		return result;
	}
}
