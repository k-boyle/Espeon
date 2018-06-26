using System;

namespace Umbreon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Usage : Attribute
    {
        public string Example { get; }

        public Usage(string example)
        {
            Example = example;
        }
    }
}
