using Discord.Commands;
using System;
using System.Threading.Tasks;
using Umbreon.Results;

namespace Umbreon.Preconditions
{
    public class RequireGuildAttribute : PreconditionAttribute
    {
        private readonly ulong _guildId;

        public RequireGuildAttribute(ulong guildId)
            => _guildId = guildId;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.Guild.Id == _guildId
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError(PreconditionResult.FromError(new FailedResult("Unknown Command", false, CommandError.UnknownCommand))));
        }
    }
}
