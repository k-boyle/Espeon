using System;

namespace Espeon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InitialiserAttribute : Attribute
    {
        public Type[] Arguments { get; }

        public InitialiserAttribute(params Type[] args)
        {
            Arguments = args;
        }
    }
}
