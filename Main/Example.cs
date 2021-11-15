using System;
using GeneratedMethods;
namespace Main
{
    [AddMethods]
    public partial class Example
    {
        public int Page { get; set; }
        public string Text { get; set; }

        public Example(int page, string text)
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
