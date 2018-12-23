using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.Checks
{
    public class RequireOwnerAttribute : CheckBaseAttribute
    {
        public override async Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
        {
            if(!(originalContext is EspeonContext context))
                throw new ExpectedContextException("EspeonContext");

            var app = await context.Client.GetApplicationInfoAsync();

            return app.Owner.Id == context.User.Id
                ? new CheckResult()
                : new CheckResult(Command is null
                    ? $"{Module.Name} commands can only be used by the bot owner"
                    : $"{Command.Name} can only be used by the bot owner");
        }
    }
}
