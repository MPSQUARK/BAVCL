using System.Numerics;

namespace BAVCL.Extensions
{
    public static partial class Extensions
    {
        private static float SimpleSum(float[] arr)
        {
            Vector<float> sumVector = Vector<float>.Zero;
            int i = 0;
            int vectorSize = Vector<float>.Count;

            for (; i <= arr.Length - vectorSize; i += vectorSize)
                sumVector += new Vector<float>(arr, i);

            float result = 0;
            for (; i < arr.Length; i++)
                result += arr[i];
            for (int j = 0; j < vectorSize; j++)
                result += sumVector[j];

            return result;
        }
        private static float CorrectingSum(float[] arr)
        {
            Vector<float> sumVector = Vector<float>.Zero;
            Vector<float> c = Vector<float>.Zero;
            Vector<float> input, y, t;
            int i = 0;
            int vectorSize = Vector<float>.Count;

            for (; i <= arr.Length - vectorSize; i += vectorSize)
            {
                input = new(arr, i);
                y = input - c;
                t = sumVector + y;

                c = (t - sumVector) - y;

                sumVector = t;
            }

            float result = 0;
            for (; i < arr.Length; i++)
                result += arr[i];
            for (int j = 0; j < vectorSize; j++)
                result += sumVector[j];

            return result;
        }    
        public static float Sum(this float[] arr)
        {
            if (arr.Length >= 1e4f)
                return CorrectingSum(arr);
            else
                return SimpleSum(arr);
        }

        private static double SimpleSum(double[] arr)
        {
            Vector<double> sumVector = Vector<double>.Zero;
            int i = 0;
            int vectorSize = Vector<float>.Count;

            for (; i <= arr.Length - vectorSize; i += vectorSize)
                sumVector += new Vector<double>(arr, i);

            double result = 0;
            for (; i < arr.Length; i++)
                result += arr[i];
            for (int j = 0; j < vectorSize; j++)
                result += sumVector[j];

            return result;
        }
        public static double Sum(this double[] arr)
        {
            return SimpleSum(arr);
        }


    }
}
