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
        public static Vector GetSliceAsVector(Vector vector, int row_col_index, Axis axis)
        {
            if (vector.Columns == 1)
                throw new Exception("Input Vector cannot be 1D");


            return axis switch
            {
                Axis.Row => vector.GetRowAsVector(row_col_index),
                Axis.Column => vector.GetColumnAsVector(row_col_index),
                _ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
        }

        /// <summary>
        /// Access a specific slice of either a column 'c' or row 'r' of a vector
        /// </summary>
        /// <param name="row_col_index"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public Vector GetSliceAsVector(int row_col_index, Axis axis)
        {
            if (Columns == 1)
                throw new Exception("Input Vector cannot be 1D");

            return axis switch
            {
                Axis.Row => GetRowAsVector(row_col_index),
                Axis.Column => GetColumnAsVector(row_col_index),
                _ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
        }


        public static float[] GetSliceAsArray(Vector vector, int row_col_index, Axis axis)
        {
            if (vector.Columns == 1)
                throw new Exception("Input Vector cannot be 1D");


            return axis switch
            {
                Axis.Row => vector.GetRowAsArray(row_col_index),
                Axis.Column => vector.GetColumnAsArray(row_col_index),
                _ => throw new Exception("Please select a valid Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
        }

        public float[] GetSliceAsArray(int row_col_index, Axis axis)
        {
            if (Columns == 1)
                throw new Exception("Input Vector cannot be 1D");

            return axis switch
            {
                Axis.Row => GetRowAsArray(row_col_index),
                Axis.Column => GetColumnAsArray(row_col_index),
                _ => throw new Exception("Please select a valid Axis This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
        }



    }


}
