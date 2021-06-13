using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public sealed class Converter
    {
        public static string ToFileFormat(Vector vector, string format = "txt")
        {
            switch (format)
            {
                case "txt":
                    return vector.ToString();
                case "csv":
                    return vector.ToCSV();
                default:
                    throw new Exception($"No format : {format} available\n Func - ConvertToFormat - IO");
            }



        }
        public static string ToFileFormat(Vector3 vector, string format = "txt")
        {
            switch (format)
            {
                case "txt":
                    return vector.ToString();
                case "csv":
                    return vector.ToCSV();
                default:
                    throw new Exception($"No format : {format} available\n Func - ConvertToFormat - IO");
            }



        }




    }



}
