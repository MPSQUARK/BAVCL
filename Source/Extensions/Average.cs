using System.Linq;


namespace BAVCL.Extensions
{
    public static partial class Extensions
    {
        public static float Average(this float[] arr) => arr.Sum() / (float)arr.Length;
        public static double Average(this double[] arr) => arr.Sum() / (double)arr.Length;
        public static float Average(this int[] arr) => (float)arr.Sum() / (float)arr.Length;
        //public static float Average(this uint[] arr) => (float)arr.Sum() / (float)arr.Length;
        public static float Average(this long[] arr) => (float)arr.Sum() / (float)arr.Length;
        //public static float Average(this ulong[] arr) => (float)arr.Sum() / (float)arr.Length;
        //public static float Average(this byte[] arr) => (float)arr.Sum() / (float)arr.Length;
        //public static float Average(this sbyte[] arr) => (float)arr.Sum() / (float)arr.Length;

    }
}
