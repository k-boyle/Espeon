using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequirePositionHierarchy : ParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext originalContext, IServiceProvider provider)
        {
            var role = argument as SocketRole;
            var context = originalContext as EspeonContext;

            return new ValueTask<CheckResult>(
                role.Position <= context.Guild.CurrentUser.Hierarchy 
                    ? CheckResult.Successful 
                    : CheckResult.Unsuccessful("I require hierarchy over this role"));
        }
    }
}
