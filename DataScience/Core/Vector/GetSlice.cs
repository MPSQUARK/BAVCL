using System;
using BAVCL.Core;

namespace BAVCL
{
    public partial class Vector
    {

        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="row_col_index"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static Vector GetSliceAsVector(Vector vector, int row_col_index, Enums.Axis axis)
        {
            if (vector.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            switch (axis)
            {
                case Enums.Axis.Row:
                    return vector.GetRowAsVector(row_col_index);
                case Enums.Axis.Column:
                    return vector.GetColumnAsVector(row_col_index);
                default:
                    throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid.");
            }
        }

        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
        /// </summary>
        /// <param name="row_col_index"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public Vector GetSliceAsVector(int row_col_index, Enums.Axis axis)
        {
            if (Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            switch (axis)
            {
                case Enums.Axis.Row:
                    return GetRowAsVector(row_col_index);
                case Enums.Axis.Column:
                    return GetColumnAsVector(row_col_index);
                default:
                    throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid.");
            }
            
        }


        public static float[] GetSliceAsArray(Vector vector, int row_col_index, Enums.Axis axis)
        {
            if (vector.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            switch (axis)
            {
                case Enums.Axis.Row:
                    return vector.GetRowAsArray(row_col_index);
                case Enums.Axis.Column:
                    return vector.GetColumnAsArray(row_col_index);
                default:
                    throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid.");
            }
        }

        public float[] GetSliceAsArray(int row_col_index, Enums.Axis axis)
        {
            if (Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            switch (axis)
            {
                case Enums.Axis.Row:
                    return GetRowAsArray(row_col_index);
                case Enums.Axis.Column:
                    return GetColumnAsArray(row_col_index);
                default:
                    throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid.");
            }


        }



    }


}
