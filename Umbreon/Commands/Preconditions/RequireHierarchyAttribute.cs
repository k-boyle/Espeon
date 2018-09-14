using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Umbreon.Commands.Preconditions
{
    public class RequireHierarchyAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            var currentUser = (context as SocketCommandContext)?.Guild.CurrentUser;
            if (value is SocketGuildUser guildUser)
            {
                return Task.FromResult(currentUser?.Hierarchy > guildUser.Hierarchy
                    ? PreconditionResult.FromSuccess(parameter.Command)
                    : PreconditionResult.FromError(parameter.Command, "You don't have hierarchy over this user"));
            }

            return Task.FromResult(PreconditionResult.FromError(parameter.Command, "This error shouldn't exist"));
        }
    }
}
