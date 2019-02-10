using System;

namespace Espeon.Commands.TypeParsers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DontOverrideAttribute : Attribute
    {
    }
}
