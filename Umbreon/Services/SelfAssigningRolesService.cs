using Discord.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Umbreon.Services
{
    public class SelfAssigningRolesService
    {
        private readonly DatabaseService _database;

        public SelfAssigningRolesService(DatabaseService database)
        {
            _database = database;
        }

        public bool HasRole(IEnumerable<ulong> roles, ulong roleToCheck)
            => roles.Contains(roleToCheck);

        public IEnumerable<ulong> GetRoles(ICommandContext context)
            => _database.GetGuild(context).SelfAssigningRoles;
    }
}
