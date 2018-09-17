using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.Preconditions
{
    public class RequireGuildAttribute : PreconditionAttribute
    {
        private readonly ulong _guildId;

        public RequireGuildAttribute(ulong guildId)
            => _guildId = guildId;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.Guild.Id == _guildId
                ? Task.FromResult(PreconditionResult.FromSuccess(command))
                : Task.FromResult(PreconditionResult.FromError(command, "Command not found"));
        }
    }
}
