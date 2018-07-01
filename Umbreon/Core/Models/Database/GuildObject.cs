using LiteDB;
using System.Collections.Generic;

namespace Umbreon.Core.Models.Database
{
    public class GuildObject
    {
        [BsonId(false)]
        public ulong GuildId { get; set; }
        
        public ulong AdminRole { get; set; }
        public ulong ModRole { get; set; }
        public Starboard Starboard { get; set; } = new Starboard();
        public List<ulong> SelfAssigningRoles { get; set; } = new List<ulong>();
        public List<Module> DisabledModules { get; set; } = new List<Module>();
        public List<string> Prefixes { get; set; } = new List<string>
        {
            "`"
        };
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<CustomCommand> CustomCommands { get; set; } = new List<CustomCommand>();
        public List<CustomFunction> CustomFunctions { get; set; } = new List<CustomFunction>();
    }
}
