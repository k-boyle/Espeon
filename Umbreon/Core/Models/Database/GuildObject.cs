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
        public ulong WelcomeChannel { get; set; } = 0; // TODO
        public string WelcomeMessage { get; set; } // TODO
        public ulong MOTDChannel { get; set; } = 0; // TODO
        public string MOTDMessage { get; set; } // TODO
        public bool CloseCommandMatching { get; set; } = false;
        public bool UseWhiteList { get; set; } = false; // TODO
        public Starboard Starboard { get; set; } = new Starboard();
        public List<ulong> SelfAssigningRoles { get; set; } = new List<ulong>();
        public List<Module> DisabledModules { get; set; } = new List<Module>();
        public List<string> Prefixes { get; set; } = new List<string>
        {
            "um!"
        };
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<CustomCommand> CustomCommands { get; set; } = new List<CustomCommand>();
        public List<CustomFunction> CustomFunctions { get; set; } = new List<CustomFunction>(); // TODO
        public List<Warning> Warnings { get; set; } = new List<Warning>(); // TODO
        public List<ulong> BlacklistedUsers { get; set; } = new List<ulong>(); // TODO
        public List<ulong> WhiteListedUsers { get; set; } = new List<ulong>(); // TODO
        public List<ulong> RestrictedChannels { get; set; } = new List<ulong>(); // TODO
    }
}
