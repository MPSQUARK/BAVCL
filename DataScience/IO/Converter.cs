using System;

namespace DataScience.IO
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
        public static string ToFileFormat(Geometric.Vector3 vector, string format = "txt")
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
