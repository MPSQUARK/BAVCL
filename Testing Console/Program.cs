using System;
using DataScience;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {


            Vector vector = Vector.Fill(5, 10, 3);

            for (int i = 0; i < vector.Value.Length; i++)
            {
                if (i % vector.Columns == 0)
                {
                    Console.WriteLine();
                }
                Console.Write($"| {vector.Value[i]} |");
            }

        }
    }
}
