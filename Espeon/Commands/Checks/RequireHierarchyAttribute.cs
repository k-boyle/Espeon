using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.Checks
{
    public class RequireHierarchyAttribute : ParameterCheckBaseAttribute
    {
        public override async Task<CheckResult> CheckAsync(object argument, ICommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;
            var targetUser = argument as IGuildUser;

            var currentGuild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild);

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

            var you = "You require hierarchy over this user";
            var i = "I need hierarchy over this user";

            if (target >= executor)
                return CheckResult.Unsuccessful(you);

            if (context.Guild.CurrentUser is null)
                throw new ThisWasQuahusFaultException();
            
            if (targetUser.Id == context.Guild.OwnerId)
                return CheckResult.Unsuccessful("You don't have hierachy over the guild owner");

            if (targetUser is SocketGuildUser socket)
            {
                if (context.Guild.CurrentUser.Hierarchy <= socket.Hierarchy)
                    return CheckResult.Unsuccessful("I need hierarchy over this user");

                return context.User.Hierarchy > socket.Hierarchy
                    ? CheckResult.Successful
                    : CheckResult.Unsuccessful(you);
            }

            var roles = targetUser.RoleIds.Select(x => context.Guild.GetRole(x));
            var ordered = roles.OrderBy(x => x.Position);

            if (context.Guild.CurrentUser.Hierarchy <= ordered.First().Position)
                return CheckResult.Unsuccessful(i);

            return context.User.Hierarchy > ordered.First().Position
                ? CheckResult.Successful
                : CheckResult.Unsuccessful(you);
        }
    }
}
