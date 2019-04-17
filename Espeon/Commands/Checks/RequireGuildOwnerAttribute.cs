using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireGuildOwnerAttribute : RequireOwnerAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext originalContext, IServiceProvider provider)
        {
            var result = await base.CheckAsync(originalContext, provider);

            if(result.IsSuccessful)
                return CheckResult.Successful;

            var context = (EspeonContext)originalContext;

            if (context.User.Id == context.Guild.OwnerId)
                return CheckResult.Successful;

            var user = context.Invoker;

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = "This command can only be executed by the guilds owner",
                [ResponsePack.owo] = "owwnnoo dis comwand can only bee exercuted bwy the gwuild owoner"
            };

            return CheckResult.Unsuccessful(resp[user.ResponsePack]);
        }
    }
}
