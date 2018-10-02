using System;

namespace Espeon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InitAttribute : Attribute
    {
        public Type[] Arguments { get; }

        public InitAttribute(params Type[] args)
        {
            Arguments = args;
        }
    }
}
