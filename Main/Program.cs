using System;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(".NET 10 Source Generator - Demo!");
            Console.WriteLine("\nRunning Method Generator:\n");
            UseMethodGenerator.Run();

            Console.WriteLine("\nRunning Property Generator:\n");
            UsePropertyGenerator.Run();
        }
    }
}
