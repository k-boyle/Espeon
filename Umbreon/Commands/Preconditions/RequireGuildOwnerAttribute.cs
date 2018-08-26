using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Umbreon.Commands.Preconditions
{
    public class RequireGuildOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.User.Id == context.Guild.OwnerId
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("Only the guild owner can execute this commands"));
        }
    }
}
