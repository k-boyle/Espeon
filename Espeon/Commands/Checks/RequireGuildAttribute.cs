using Qmmands;
using System;
using System.Collections.Generic;
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

            if (context.Guild.Id == _id)
                return CheckResult.Successful;

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = "Command cannot be run in this guild",
                [ResponsePack.owo] = "ownno dis cwommand cant be wun is dis gwuild"
            };

            return CheckResult.Unsuccessful(resp[context.Invoker.ResponsePack]);
        }
    }
}
