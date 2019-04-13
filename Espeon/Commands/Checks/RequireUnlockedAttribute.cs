using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireUnlockedAttribute : ParameterCheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(object argument, CommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            if (!(argument is ResponsePack pack))
                throw new InvalidOperationException("Check can only be used on parameters of type ResponsePack");

            var user = await context.UserStore.GetOrCreateUserAsync(context.User);

            if(user.ResponsePacks.Any(x => x == pack))
                return CheckResult.Successful;

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = "You haven't unlocked this response pack",
                [ResponsePack.owo] = "ownnno uu dunt hav dis respwons pack"
            };

            return CheckResult.Unsuccessful(resp[user.ResponsePack]);
        }
    }
}
