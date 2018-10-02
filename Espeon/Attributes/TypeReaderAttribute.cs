using System;

namespace Espeon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeReaderAttribute : Attribute
    {
        public Type TargetType { get; }

        public TypeReaderAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}
