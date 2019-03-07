using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequirePositionHierarchy : ParameterCheckBaseAttribute
    {
        public override Task<CheckResult> CheckAsync(object argument, ICommandContext originalContext, IServiceProvider provider)
        {
            var role = argument as SocketRole;
            var context = originalContext as EspeonContext;

            return Task.FromResult(
                role.Position <= context.Guild.CurrentUser.Hierarchy 
                    ? CheckResult.Successful 
                    : CheckResult.Unsuccessful("I require hierarchy over this role"));
        }
    }
}
