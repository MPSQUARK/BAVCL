using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using BAVCL.Core;

namespace BAVCL.IO
{
    public class IO
    {
        // WRITE TO FILE

        // GENERIC
        public static void WriteToFile(string input, string filename="new file", string format="txt" ,string path = "")
        {
            if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
            Directory.CreateDirectory(path+@"\saved_data\");
            string fullpath = $"{path}/saved_data/{filename}.{format}";

            using (StreamWriter writer = new(fullpath))
            {
                writer.Write(input);
            }

            return;
        }
        public static void WriteToFile(IIO IOable, string filename = "new file", string format = "txt", string path = "")
        {
            if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
            Directory.CreateDirectory(path + @"\saved_data\");
            string fullpath = $"{path}/saved_data/{filename}.{format}";

            using (StreamWriter writer = new(fullpath))
            {
                writer.Write(ToFileFormat(IOable, format));
            }

            return;
        }


        public static string ToFileFormat(IIO writable, string format = "txt")
        {
            return format switch
            {
                "txt" => writable.ToString(),
                "csv" => writable.ToCSV(),
                _ => throw new Exception($"No format : {format} available\n Func - ConvertToFormat - IO"),
            };
        }


        // READ FROM FILE

        public static Vector CSV2Vector(GPU gpu, string filename, string format="csv", string path="")
        {
            if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
            
            string contents = File.ReadAllText($"{path}/saved_data/{filename}.{format}");

            // CSV

            int columns = Regex.Match(contents, @"^.*?(?=\n)").ToString().Split(",").Length-1;

            float[] Output = Array.ConvertAll(contents.Replace("\r\n", "").Split(",").ToArray()[..^1], float.Parse);

            return new Vector(gpu, Output, columns);
        }



    }



}
