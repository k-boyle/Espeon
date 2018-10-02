using System;
using Espeon.Core;

namespace Espeon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {

        public ServiceType Type { get; }

        public ServiceAttribute(ServiceType type = ServiceType.Singleton)
        {
            Type = type;
        }
    }
}
