using Microsoft.Extensions.DependencyInjection;
using System;

namespace Espeon.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public Type Target { get; }
        public bool Implement { get; }
        public ServiceLifetime Lifetime { get; }
        
        public ServiceAttribute(Type target, ServiceLifetime lifetime, bool implement)
        {
            if(!target.IsInterface)
                throw new NotInterfaceException($"{nameof(target)} must be an interface");

            Target = target;
            Implement = implement;
            Lifetime = lifetime;
        }
    }
}
