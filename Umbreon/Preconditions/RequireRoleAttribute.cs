using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Core;
using Umbreon.Extensions;
using Umbreon.Services;

namespace Umbreon.Preconditions
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly SpecialRole _role;

        public RequireRoleAttribute(SpecialRole role)
            => _role = role;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetService<DatabaseService>();
            var guild = database.GetGuild(context);
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
                return Task.FromResult(PreconditionResult.FromError($"{_role} role not found. Please do `{guild.Prefixes.First()}set {_role}Role` to setup this role"));
            var user = context.User as SocketGuildUser;
            return user.HasRole(roleId)
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("You do not have the required role"));
        }
    }
}
