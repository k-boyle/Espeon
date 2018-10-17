using Espeon.Interfaces;
using System.Collections.Generic;

namespace Espeon.Core.Entities.New
{
    public class GuildObject : BaseObject
    {
        public GuildObject(GuildObject baseObj, IRemoveableService service) : base(baseObj, service)
        {
            SpecialRoles = baseObj.SpecialRoles;
            Configuration = baseObj.Configuration;
            Data = baseObj.Data;
        }

        public GuildObject() { }

        public SpecialRoles SpecialRoles { get; set; } = new SpecialRoles();

        public GuildConfiguration Configuration { get; set; } = new GuildConfiguration();

        public GuildData Data { get; set; } = new GuildData();
    }

    public class SpecialRoles
    {
        public ulong AdminRoleId { get; set; }
        public ulong ModRoleId { get; set; }
    }

    public class GuildConfiguration
    {
        public ulong WelcomeChannelId { get; set; }
        public string WelcomeMessage { get; set; }

        public List<string> Prefixes { get; set; } = new List<string>
        {
            "es?"
        };

        public List<ulong> RestrictedChannelIds { get; set; } = new List<ulong>();
    }

    public class GuildData
    {
        public List<ulong> RoleIds = new List<ulong>();

        public List<CustomCommand> CustomCommands = new List<CustomCommand>();
    }

    public class CustomCommand
    {
        public string CommandName { get; set; }
        public string CommandValue { get; set; }
    }
}
