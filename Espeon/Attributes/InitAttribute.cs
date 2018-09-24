using System;

namespace Espeon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InitAttribute : Attribute
    {
        public readonly Type[] Arguments;

        public InitAttribute(params Type[] args)
        {
            Arguments = args;
        }
    }
}
