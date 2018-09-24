using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Espeon.Core;
using Espeon.Core.Entities.Guild;
using Espeon.Extensions;
using Espeon.Services;

namespace Espeon.Commands.Preconditions
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly SpecialRole _role;

        public RequireRoleAttribute(SpecialRole role)
            => _role = role;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetService<DatabaseService>();
            var guild = await database.GetObjectAsync<GuildObject>("guilds", context.Guild.Id);
            ulong roleId;
            switch (_role)
            {
                case SpecialRole.Admin:
                    roleId = guild.AdminRole;
                    break;

                case SpecialRole.Mod:
                    roleId = guild.ModRole;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (roleId == 0 || !context.Guild.Roles.Select(x => x.Id).Contains(roleId))
                return PreconditionResult.FromError(command, $"{_role} role not found. Please do `{guild.Prefixes.First()}set {_role}Role` to setup this role");
            var user = context.User as SocketGuildUser;
            return user.HasRole(roleId)
                ? PreconditionResult.FromSuccess(command)
                : PreconditionResult.FromError(command, "You do not have the required role");
        }
    }
}
