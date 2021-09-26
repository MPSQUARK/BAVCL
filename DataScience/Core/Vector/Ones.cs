
using System.Linq;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Ones(GPU gpu, int Length, int Columns = 1)
        {
            return new Vector(gpu, Enumerable.Repeat(1f, Length).ToArray(), Columns);
        }
        public void Ones_IP(int Length, int Columns = 1)
        {
            this.Value = Enumerable.Repeat(1f, Length).ToArray();
            this.Columns = Columns;
            return;
        }

    }
}
