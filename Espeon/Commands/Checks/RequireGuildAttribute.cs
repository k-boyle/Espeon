using Qmmands;
using System;
using System.Threading.Tasks;
using Espeon.Core;

namespace Espeon.Commands.Checks
{
    public class RequireGuildAttribute : CheckBaseAttribute
    {
        private readonly ulong _id;

        public RequireGuildAttribute(ulong id)
        {
            _id = id;
        }

        public override Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
        {
            if (!(originalContext is EspeonContext context))
                throw new ExpectedContextException("IEspeonContext");

            return Task.FromResult(context.Guild.Id == _id
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("Command cannot be run in this guild"));
        }
    }
}
