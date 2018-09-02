using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using Umbreon.Attributes;
using Umbreon.Core.Entities.Guild;

namespace Umbreon.Services
{
    [Service]
    public class SelfAssigningRolesService
    {
        private readonly DatabaseService _database;

        public SelfAssigningRolesService(DatabaseService database)
        {
            _database = database;
        }

        public void AddNewSelfRole(ICommandContext context, ulong roleId)
        {
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.SelfAssigningRoles.Add(roleId);
            _database.UpdateObject(guild, "guilds");
        }

        public void RemoveSelfRole(ICommandContext context, ulong roleId)
        {
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.SelfAssigningRoles.Remove(roleId);
            _database.UpdateObject(guild, "guilds");
        }

        public static bool HasRole(IEnumerable<ulong> roles, ulong roleToCheck)
            => roles.Contains(roleToCheck);

        public IEnumerable<ulong> GetRoles(ICommandContext context)
            => _database.GetObject<GuildObject>("guilds", context.Guild.Id).SelfAssigningRoles;
    }
}
