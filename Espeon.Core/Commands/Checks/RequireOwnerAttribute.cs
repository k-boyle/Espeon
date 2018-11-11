using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Checks
{
    public class RequireOwnerAttribute : CheckBaseAttribute
    {
        public override async Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
        {
            if(!(originalContext is IEspeonContext context))
                return new ContextResult();

            var owner = await context.Client.GetApplicationInfoAsync();

            return owner.Id == context.User.Id
                ? new CheckResult()
                : new CheckResult(Command is null
                    ? $"{Module.Name} commands can only be used by the bot owner"
                    : $"{Command.Name} can only be used by the bot owner");
        }
    }
}
