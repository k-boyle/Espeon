using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireUnlockedAttribute : ParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext originalContext, IServiceProvider provider)
        {
            var context = (EspeonContext)originalContext;
            var response = provider.GetService<ResponseService>();

            if (!(argument is ResponsePack pack))
                throw new InvalidOperationException("Check can only be used on parameters of type ResponsePack");

            var user = context.Invoker;

            return user.ResponsePacks.Any(x => x == pack) 
                ? CheckResult.Successful 
                : CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
