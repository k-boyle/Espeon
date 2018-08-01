using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using Umbreon.Attributes;

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
            var guild = _database.GetGuild(context);
            guild.SelfAssigningRoles.Add(roleId);
            _database.UpdateGuild(guild);
        }

        public void RemoveSelfRole(ICommandContext context, ulong roleId)
        {
            var guild = _database.GetGuild(context);
            guild.SelfAssigningRoles.Remove(roleId);
            _database.UpdateGuild(guild);
        }

        public bool HasRole(IEnumerable<ulong> roles, ulong roleToCheck)
            => roles.Contains(roleToCheck);

        public IEnumerable<ulong> GetRoles(ICommandContext context)
            => _database.GetGuild(context).SelfAssigningRoles;
    }
}
