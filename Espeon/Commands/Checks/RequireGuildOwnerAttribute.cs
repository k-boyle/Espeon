using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
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
            var response = provider.GetService<ResponseService>();

            return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
