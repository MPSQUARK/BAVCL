using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using DataScience.Utility;

namespace DataScience
{
    namespace IO
    {
        public class IO
        {

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
                string input  = Util.ConvertToFormat(vector, format);

                using (StreamWriter writer = new StreamWriter(fullpath))
                {
                    writer.Write(input);
                }

                return;
            }
            public static void WriteToFile(Vector3 vector, string filename = "new file", string format = "txt", string path = "")
            {
                if (path == "") { path = AppDomain.CurrentDomain.BaseDirectory; }
                System.IO.Directory.CreateDirectory(path + @"\saved_data\");
                string fullpath = path + @"\saved_data\" + filename + "." + format;
                string input = Util.ConvertToFormat(vector, format);

                using (StreamWriter writer = new StreamWriter(fullpath))
                {
                    writer.Write(input);
                }

                return;
            }


            public static void WriteToCSV(string input, string filename="new file", string path = "")
            {
                WriteToFile(input, filename, "csv", path);
            }
            public static void WriteToCSV(Vector vector, string filename = "new file", string path = "")
            {
                WriteToFile(vector.ToCSV(), filename, "csv", path);
            }
            public static void WriteToCSV(Vector3 vector, string filename = "new file", string path = "")
            {
                WriteToFile(vector.ToCSV(), filename, "csv", path);
            }







        }

    }

}
