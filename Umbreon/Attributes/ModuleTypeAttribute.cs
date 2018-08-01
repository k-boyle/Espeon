using System;
using Umbreon.Core;

namespace Umbreon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleTypeAttribute : Attribute
    {
        public Module Type;

        public ModuleTypeAttribute(Module module)
            => Type = module;
    }
}
