using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {

        public string ToCSV()
        {
            SyncCPU();
            StringBuilder stringBuilder = new StringBuilder();


            if (Columns != 1)
            {
                for (int i = 0; i < Length; i++)
                {
                    stringBuilder.Append($"{this.Value[i]},");
                }
                return stringBuilder.ToString();
            }


            stringBuilder.Append($"{this.Value[0]},");

            for (int i = 1; i < Length; i++)
            {
                if (i % Columns == 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append($"{this.Value[i]},");
            }

            return stringBuilder.ToString();
        }



    }


}
