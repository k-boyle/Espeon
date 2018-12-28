using System.Collections.Generic;

namespace Espeon.Database.Entities
{
    public class ModuleInfo : DatabaseEntity
    {
        public override ulong Id { get; set; }

        public string Name { get; set; }

        public List<string> Aliases { get; set; } = new List<string>();
        public List<CommandInfo> Commands { get; set; } = new List<CommandInfo>();

        public override long WhenToRemove { get; set; }
    }

    public class CommandInfo
    { 
        public string Id { get; set; }

        public string Name { get; set; }
        public List<string> Aliases { get; set; } = new List<string>();
    }
}
