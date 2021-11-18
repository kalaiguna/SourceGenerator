using System;
using MethodGenerator;
namespace Main
{
    [AddMethods]
    public partial class MethodExample
    {
        public int Page { get; set; }
        public string Text { get; set; }

        public MethodExample(int page, string text)
        {
            Page = page;
            Text = text;
        }
        public void UserMethod()
        {
            Console.WriteLine("Invocation of User written Method!\n");
            Console.WriteLine($"Values are:  Page = {Page} and Text = {Text}\n");
        }

    }
}
