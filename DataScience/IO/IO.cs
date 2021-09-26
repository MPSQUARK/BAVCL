using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace DataScience.IO
{
    public class IO
    {
        // WRITE TO FILE

        // GENERIC
        public static void WriteToFile(string input, string filename="new file", string format="txt" ,string path = "")
        {
            if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
            System.IO.Directory.CreateDirectory(path+@"\saved_data\");
            string fullpath = path + @"\saved_data\" + filename + "." + format;

            using (StreamWriter writer = new StreamWriter(fullpath))
            {
                writer.Write(input);
            }

            return;
        }
        public static void WriteToFile(Vector vector, string filename = "new file", string format = "txt", string path = "")
        {
            if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
            System.IO.Directory.CreateDirectory(path + @"\saved_data\");
            string fullpath = path + @"\saved_data\" + filename + "." + format;
            string input  = Converter.ToFileFormat(vector, format);

            using (StreamWriter writer = new StreamWriter(fullpath))
            {
                writer.Write(input);
            }

            return;
        }
        public static void WriteToFile(Geometric.Vector3 vector, string filename = "new file", string format = "txt", string path = "")
        {
            if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
            System.IO.Directory.CreateDirectory(path + @"\saved_data\");
            string fullpath = path + @"\saved_data\" + filename + "." + format;
            string input = Converter.ToFileFormat(vector, format);

            using (StreamWriter writer = new StreamWriter(fullpath))
            {
                writer.Write(input);
            }

            return;
        }


        // FORMAT SPECIFIC

        // CSV
        public static void WriteToCSV(string input, string filename="new file", string path = "")
        {
            WriteToFile(input, filename, "csv", path);
        }
        public static void WriteToCSV(Vector vector, string filename = "new file", string path = "")
        {
            WriteToFile(vector.ToCSV(), filename, "csv", path);
        }
        public static void WriteToCSV(Geometric.Vector3 vector, string filename = "new file", string path = "")
        {
            WriteToFile(vector.ToCSV(), filename, "csv", path);
        }

        // TXT



        // JSON



        // FITS??



        // READ FROM FILE

        public static Vector ReadAsVectorFromCSV(GPU gpu, string filename, string format="csv", string path="")
        {
            if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
            
            string contents = File.ReadAllText(path + @"\saved_data\" + filename + "." + format);

            // CSV

            int columns = Regex.Match(contents, @"^.*?(?=\n)").ToString().Split(",").Length-1;

            float[] Output = Array.ConvertAll(contents.Replace("\r\n", "").Split(",").ToArray()[..^1], float.Parse);

            return new Vector(gpu, Output, columns);
        }

        public static void ReadAsVector3(GPU gpu, string filename, string path, string format)
        {

        }
        public static void ReadAsVertex(string filename, string path, string format)
        {

        }



    }



}
