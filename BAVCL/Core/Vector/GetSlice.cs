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

            return axis switch
            {
                Enums.Axis.Row => vector.GetRowAsVector(row_col_index),
                Enums.Axis.Column => vector.GetColumnAsVector(row_col_index),
                _ => throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
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

            return axis switch
            {
                Enums.Axis.Row => GetRowAsVector(row_col_index),
                Enums.Axis.Column => GetColumnAsVector(row_col_index),
                _ => throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
        }


        public static float[] GetSliceAsArray(Vector vector, int row_col_index, Enums.Axis axis)
        {
            if (vector.Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            return axis switch
            {
                Enums.Axis.Row => vector.GetRowAsArray(row_col_index),
                Enums.Axis.Column => vector.GetColumnAsArray(row_col_index),
                _ => throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
        }

        public float[] GetSliceAsArray(int row_col_index, Enums.Axis axis)
        {
            if (Columns == 1)
            {
                throw new Exception("Input Vector cannot be 1D");
            }

            return axis switch
            {
                Enums.Axis.Row => GetRowAsArray(row_col_index),
                Enums.Axis.Column => GetColumnAsArray(row_col_index),
                _ => throw new Exception("Please select a valid Axis from Enums.Axis. This is a 2D Vector so ONLY Row and Column axis are valid."),
            };
        }



    }


}
