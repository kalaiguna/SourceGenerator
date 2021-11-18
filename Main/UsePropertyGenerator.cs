using System;

namespace Main
{
    public static class UsePropertyGenerator
    {
        public static void Run()
        {
            PropertyExample foo = new();
            Console.WriteLine("* Invocation of (Source Generator Generated) Properties with Values  *\n");
            // source generator generated Properties
            foo.Page = 100;
            foo.Text = "Hallo!";
            Console.WriteLine($"Page property has value {foo.Page}\n");
            Console.WriteLine($"Text property has value {foo.Text}\n");
        }
    }
}
