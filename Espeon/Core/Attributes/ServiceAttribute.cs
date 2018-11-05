using System;

namespace Espeon.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public Type Target { get; }

        public ServiceAttribute(Type target)
        {
            if(!target.IsInterface)
                throw new NotInterfaceException($"{nameof(target)} must be an interface");

            Target = target;
        }
    }
}
