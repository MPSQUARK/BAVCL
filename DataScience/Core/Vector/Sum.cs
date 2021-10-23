namespace DataScience
{
    public partial class Vector
    {
        public override float Sum()
        {
            SyncCPU();

            int vectorSize = System.Numerics.Vector<float>.Count;
            int i = 0;
            float[] array = this.Value;

            System.Numerics.Vector<float> sumVector = System.Numerics.Vector<float>.Zero;

            if (array.Length >= 1e4f)
            {
                System.Numerics.Vector<float> c = System.Numerics.Vector<float>.Zero;
                for (; i <= array.Length - vectorSize; i += vectorSize)
                {

                    System.Numerics.Vector<float> input = new System.Numerics.Vector<float>(array, i);

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
                    System.Numerics.Vector<float> v = new System.Numerics.Vector<float>(array, i);

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

}
