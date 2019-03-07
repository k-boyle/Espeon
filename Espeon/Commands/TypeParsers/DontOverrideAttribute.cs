using System;

namespace Espeon.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DontOverrideAttribute : Attribute
    {
    }
}
