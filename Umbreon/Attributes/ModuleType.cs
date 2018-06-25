using System;
using Umbreon.Core;

namespace Umbreon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleType : Attribute
    {
        public Module Type;

        public ModuleType(Module module)
            => Type = module;
    }
}
