using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequireHierarchyAttribute : EspeonParameterCheckBase
    {
        public override async ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context, IServiceProvider provider)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();

            var targetUser = (IGuildUser)argument;

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
            var response = provider.GetService<IResponseService>();

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

            if (context.Guild.CurrentUser.Hierarchy <= ordered[0].Position)
                return CheckResult.Unsuccessful(response.GetResponse(this, p, 1));

            return context.User.Hierarchy > ordered[0].Position
                ? CheckResult.Successful
                : CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
        }
    }
}
