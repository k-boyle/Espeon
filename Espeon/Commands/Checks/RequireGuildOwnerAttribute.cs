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

            var context = originalContext as EspeonContext;

            return context.User.Id == context!.Guild.OwnerId
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("This command can only be executed by the guilds owner");
        }
    }
}
