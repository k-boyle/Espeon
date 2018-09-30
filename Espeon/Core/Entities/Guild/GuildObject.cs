using System.Collections.Generic;
using Espeon.Interfaces;

namespace Espeon.Core.Entities.Guild
{
    public class GuildObject : BaseObject
    {
        public GuildObject(GuildObject baseObj, IRemoveableService service) : base(baseObj, service)
        {
            AdminRole = baseObj.AdminRole;
            ModRole = baseObj.ModRole;
            WelcomeChannel = baseObj.WelcomeChannel;
            WelcomeMessage = baseObj.WelcomeMessage;
            UseWhiteList = baseObj.UseWhiteList;
            SelfAssigningRoles = baseObj.SelfAssigningRoles;
            Prefixes = baseObj.Prefixes;
            CustomCommands = baseObj.CustomCommands;
            BlacklistedUsers = baseObj.BlacklistedUsers;
            WhiteListedUsers = baseObj.WhiteListedUsers;
            RestrictedChannels = baseObj.RestrictedChannels;
            MusicUsers = baseObj.MusicUsers;
            Reminders = baseObj.Reminders;
        }

        public GuildObject() { }

        public ulong AdminRole { get; set; }
        public ulong ModRole { get; set; }
        public ulong WelcomeChannel { get; set; } // TODO
        public string WelcomeMessage { get; set; } // TODO
        public bool UseWhiteList { get; set; } // TODO
        public List<ulong> SelfAssigningRoles { get; set; } = new List<ulong>();
        public List<string> Prefixes { get; set; } = new List<string>
        {
            "es/"
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
