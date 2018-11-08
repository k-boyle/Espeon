using System;

namespace Espeon.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public Type Target { get; }
        public Type Generic { get; }
        public bool Implement { get; }

        public ServiceAttribute(Type target, Type generic, bool implement)
        {
            if (!target.IsInterface)
                throw new NotInterfaceException($"{nameof(target)} must be an interface");

            //if (!generic.IsInterface)
                //throw new NotInterfaceException($"{nameof(generic)} must be an interface");

            Target = target;
            Generic = generic;
            Implement = implement;
        }

        public ServiceAttribute(Type target, bool implement)
        {
            if(!target.IsInterface)
                throw new NotInterfaceException($"{nameof(target)} must be an interface");

            Target = target;
            Implement = implement;
        }
    }
}
