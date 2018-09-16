using System.Collections.Generic;
using Umbreon.Interfaces;

namespace Umbreon.Core.Entities.Guild
{
    public class GuildObject : BaseObject
    {
        public GuildObject(BaseObject baseObj, IRemoveableService service) : base(baseObj, service)
        {
        }

        public GuildObject() { }

        public ulong AdminRole { get; set; }
        public ulong ModRole { get; set; }
        public ulong WelcomeChannel { get; set; } = 0; // TODO
        public string WelcomeMessage { get; set; } // TODO
        public bool UseWhiteList { get; set; } = false; // TODO
        public List<ulong> SelfAssigningRoles { get; set; } = new List<ulong>();
        public List<string> Prefixes { get; set; } = new List<string>
        {
            "um!"
        };
        public List<CustomCommand> CustomCommands { get; set; } = new List<CustomCommand>();
        public List<ulong> BlacklistedUsers { get; set; } = new List<ulong>(); // TODO
        public List<ulong> WhiteListedUsers { get; set; } = new List<ulong>
        {
            84291986575613952
        }; // TODO
        public List<ulong> RestrictedChannels { get; set; } = new List<ulong>(); // TODO
        public List<ulong> MusicUsers { get; set; } = new List<ulong>
        {
            84291986575613952
        };
        public List<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}
