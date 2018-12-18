using Espeon.Core.Commands.Bases;
using Espeon.Core.Services;
using Qmmands;

namespace Espeon.Core.Commands.Modules
{
    public abstract class ModuleManagement : AddRemoveBase<Module, string>
    {
        public abstract IModuleManager Manager { get; set; }
    }
    
    public abstract class CommandManagement : AddRemoveBase<Command, string>
    {
        public abstract IModuleManager Manager { get; set; }
    }
}

