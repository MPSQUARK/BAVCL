using System;
using DataScience;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {


            Vector vectorA = Vector.Linspace(-10, 10, 5);
            Vector vectorB = Vector.Arange(-10, 10, 5);


            for (int i = 0; i < vectorA.Value.Length; i++)
            {
                if (i % vectorA.Columns == 0)
                {
                    Console.WriteLine();
                }
                Console.Write($"| {vectorA.Value[i]} |");
            }

            Console.WriteLine();
            Console.WriteLine();

            for (int i = 0; i < vectorB.Value.Length; i++)
            {
                if (i % vectorB.Columns == 0)
                {
                    Console.WriteLine();
                }
                Console.Write($"| {vectorB.Value[i]} |");
            }

        }
    }
}
