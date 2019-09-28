using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequireUnlockedAttribute : EspeonParameterCheckBase
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context, IServiceProvider provider)
        {
            var response = provider.GetService<IResponseService>();

            var pack = (ResponsePack) argument;

            var user = context.Invoker;

            return user.ResponsePacks.Any(x => x == pack)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
