using System;

namespace Espeon.Services
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
    }
}
