using Discord.WebSocket;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
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
            var context = (EspeonContext)originalContext;
            var response = provider.GetService<ResponseService>();

            if (role.Position <= context.Guild.CurrentUser.Hierarchy)
                    return CheckResult.Successful;

            var user = context.Invoker;

            return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
