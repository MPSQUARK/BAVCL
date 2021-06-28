using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public abstract class VectorBase<T>
    {
        protected GPU gpu { get; set; }
        public abstract T[] Value { get; set; }
        public abstract int Columns { get; protected set; }
        

        // PRINT + CSV
        public static void Print(Vector vector)
        {
            Console.WriteLine();
            Console.Write(vector.ToString());
            return;
        }
        public void Print()
        {
            Console.WriteLine();
            Console.Write(this.ToString());
            return;
        }
        public string ToCSV()
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool is1D = !(this.Columns == 1);
            for (int i = 0; i < this.Value.Length; i++)
            {
                if ((i % this.Columns == 0) && is1D && i != 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append($"{this.Value[i].ToString()},");
            }
            return stringBuilder.ToString();
        }


        // MATHEMATICAL PROPERTIES 
        public int RowCount()
        {
            if (this.Columns == 1)
            {
                return 1;
            }
            return this.Value.Length / this.Columns;
        }
        public int ColumnCount()
        {
            if (this.Columns == 1)
            {
                return 0;
            }
            return this.Columns;
        }
        public int Length()
        {
            return this.Value.Length;
        }
        public (int, int) Shape()
        {
            return (this.RowCount(), this.ColumnCount());
        }
        public void SetColumns(int Columns)
        {
            this.Columns = Columns;
        }

        public abstract T Max();
        public abstract T Min();
        public abstract T Mean();
        public abstract T Range();
        public abstract T Sum();

 



    }


}
