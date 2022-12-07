using System.Linq;

namespace BAVCL
{
    public partial class Vector
    {
        /// <summary>
        /// Creates a UNIFORM Vector where all values are equal to Value
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Length"></param>
        /// <param name="Columns"></param>
        /// <returns></returns>
        public static Vector Fill(GPU gpu, float Value, int Length, int Columns = 1) =>
            new(gpu, Enumerable.Repeat(Value, Length).ToArray(), Columns);


        /// <summary>
        /// Sets all values in THIS Vector to value, of a set size and columns
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Length"></param>
        /// <param name="Columns"></param>
        public Vector Fill_IP(float Value, int Length, int Columns = 1)
        {
            UpdateCache(Enumerable.Repeat(Value, Length).ToArray());
            this.Columns = Columns;
            return this;
        }
    }
}
