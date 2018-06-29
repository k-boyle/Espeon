using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Core;
using Umbreon.Services;

namespace Umbreon.Preconditions
{
    public class RequireRole : PreconditionAttribute
    {
        private readonly SpecialRole _role;

        public RequireRole(SpecialRole role)
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
            var user = context.User as SocketGuildUser;
            var roles = user.Roles.Select(x => x.Id);
            return roles.Contains(roleId)
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("You do not have the required role"));
        }
    }
}
