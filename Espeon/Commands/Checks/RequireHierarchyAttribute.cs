using Discord;
using Discord.WebSocket;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireHierarchyAttribute : ParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext originalContext, IServiceProvider provider)
        {
            var context = (EspeonContext)originalContext;
            var targetUser = argument as IGuildUser;

            var currentGuild = context.CurrentGuild;

            var executor = currentGuild.Admins.Contains(context.User.Id) 
                ? ElevationLevel.Admin 
                : currentGuild.Moderators.Contains(context.User.Id) 
                    ? ElevationLevel.Mod 
                    : ElevationLevel.None;

            var target = currentGuild.Admins.Contains(targetUser.Id)
                ? ElevationLevel.Admin
                : currentGuild.Moderators.Contains(targetUser.Id)
                    ? ElevationLevel.Mod
                    : ElevationLevel.None;

            if (context.Guild.CurrentUser is null)
                throw new ThisWasQuahusFaultException();

            var user = context.Invoker;
            var p = user.ResponsePack;
            var response = provider.GetService<ResponseService>();

            if (targetUser.Id == context.Guild.OwnerId)
                return CheckResult.Unsuccessful(response.GetResponse(this, p, 0));

            if (target >= executor)
                return CheckResult.Unsuccessful(response.GetResponse(this, p, 2));

            if (targetUser is SocketGuildUser socket)
            {
                if (context.Guild.CurrentUser.Hierarchy <= socket.Hierarchy)
                    return CheckResult.Unsuccessful(response.GetResponse(this, p, 1));

                return context.User.Hierarchy > socket.Hierarchy
                    ? CheckResult.Successful
                    : CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
            }

            var roles = targetUser.RoleIds.Select(x => context.Guild.GetRole(x));
            var ordered = roles.OrderBy(x => x.Position).ToArray();

            if (context.Guild.CurrentUser.Hierarchy <= ordered.First().Position)
                return CheckResult.Unsuccessful(response.GetResponse(this, p, 1));

            return context.User.Hierarchy > ordered.First().Position
                ? CheckResult.Successful
                : CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
        }
    }
}
