using System;

namespace DataScience
{
    public partial class Vector
    {

        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="row_col_index"></param>
        /// <param name="row_col"></param>
        /// <returns></returns>
        public static Vector AccessSlice(Vector vector, int row_col_index, char row_col)
        {
            if (vector.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            if (row_col == 'r')
            {
                return AccessRow(vector, row_col_index);
            }
            if (row_col == 'c')
            {
                return AccessColumn(vector, row_col_index);
            }

            throw new Exception("Invalid slice char selector, choose 'r' for row or 'c' for column");
        }

        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
        /// </summary>
        /// <param name="row_col_index"></param>
        /// <param name="row_col"></param>
        /// <returns></returns>
        public Vector AccessSlice(int row_col_index, char row_col)
        {
            if (this.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            if (row_col == 'r')
            {
                return new Vector(this.gpu, AccessRow(row_col_index), 1);
            }
            if (row_col == 'c')
            {
                return AccessColumn(row_col_index);
            }

            throw new Exception("Invalid slice char selector, choose 'r' for row or 'c' for column");
        }


    }


}
