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
        public override Task<CheckResult> CheckAsync(object argument, ICommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;
            var targetUser = argument as IGuildUser;

            if (context.Guild.CurrentUser is null)
                throw new ThisWasQuahusFaultException();
            
            if (targetUser.Id == context.Guild.OwnerId)
                return Task.FromResult(CheckResult.Unsuccessful("You can't kick the guild owner"));

            if (targetUser is SocketGuildUser socket)
            {
                if (context.Guild.CurrentUser.Hierarchy <= socket.Hierarchy)
                    return Task.FromResult(CheckResult.Unsuccessful("I need hierarchy over this user"));

                return Task.FromResult(context.User.Hierarchy > socket.Hierarchy
                    ? CheckResult.Successful
                    : CheckResult.Unsuccessful("You require hierarchy over this user"));
            }

            var roles = targetUser.RoleIds.Select(x => context.Guild.GetRole(x));
            var ordered = roles.OrderBy(x => x.Position);

            if (context.Guild.CurrentUser.Hierarchy <= ordered.First().Position)
                return Task.FromResult(CheckResult.Unsuccessful("I need hierarchy over this user"));

            return Task.FromResult(context.User.Hierarchy > ordered.First().Position
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("You require hierarchy over this user"));
        }
    }
}
