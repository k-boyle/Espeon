using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequirePositionHierarchy : EspeonParameterCheckBase
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context, IServiceProvider provider)
        {
            var role = (SocketRole)argument;
            var response = provider.GetService<IResponseService>();

            if (role.Position <= context.Guild.CurrentUser.Hierarchy)
                    return CheckResult.Successful;

            var user = context.Invoker;

            return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
