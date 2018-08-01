using System;

namespace Umbreon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UsageAttribute : Attribute
    {
        public string Example { get; }

        public UsageAttribute(string example)
        {
            Example = example;
        }
    }
}
