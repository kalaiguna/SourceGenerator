using System;

namespace Main
{
    public static class UseMethodGenerator
    {
        public static void Run()
        {
            MethodExample foo = new(1, "lowertoupper");
            // regular method
            foo.UserMethod();

            Console.WriteLine("* Invocation of (Source Generator Generated) Methods  *\n");
            // source generator generated methods
            Console.WriteLine($"Default Method invoked returns {foo.Default()}\n");
            foo.LowerToUpper();
            foo.NextPage();
            foo.GeneratedMethod();
        }
    }
}
