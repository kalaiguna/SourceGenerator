using System;
using PropertyGenerator;
namespace Main
{
    public partial class PropertyExample
    {
        [AddProperty]
        private int page;

        [AddProperty]
        private string _text;

        private bool flag;
    }
}
