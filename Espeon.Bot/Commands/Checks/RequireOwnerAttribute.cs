using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequireOwnerAttribute : EspeonCheckBase
    {
        public override async ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider)
        {
            var response = provider.GetService<IResponseService>();

            var app = await context.Client.GetApplicationInfoAsync();

            if (app.Owner.Id == context.User.Id || context.Client.CurrentUser.Id == context.User.Id)
                return CheckResult.Successful;

            var user = context.Invoker;

            return CheckResult.Unsuccessful(Command is null
                ? response.GetResponse(this, user.ResponsePack, 0, Module?.Name)
                : response.GetResponse(this, user.ResponsePack, 1, Command?.Name));
        }
    }
}
