using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequirePositionHierarchy : ParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext originalContext, IServiceProvider provider)
        {
            var role = argument as SocketRole;
            var context = (EspeonContext)originalContext;

            if (role.Position <= context.Guild.CurrentUser.Hierarchy)
                    return CheckResult.Successful;

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = "I require hierarchy over this role",
                [ResponsePack.owo] = "oww this wole is twoo big 4 mee ><"
            };

            var user = context.Invoker;

            return CheckResult.Unsuccessful(resp[user.ResponsePack]);
        }
    }
}
