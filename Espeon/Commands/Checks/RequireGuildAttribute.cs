using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireGuildAttribute : CheckAttribute
    {
        private readonly ulong _id;

        public RequireGuildAttribute(ulong id)
        {
            _id = id;
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext originalContext, IServiceProvider provider)
        {
            var context = (EspeonContext)originalContext;
            var response = provider.GetService<ResponseService>();

            return context.Guild.Id == _id 
                ? CheckResult.Successful 
                : CheckResult.Unsuccessful(response.GetResponse(this, context.Invoker.ResponsePack, 0));
        }
    }
}
