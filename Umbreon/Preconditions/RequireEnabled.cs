using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Umbreon.Preconditions
{
    public class RequireEnabled : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
