using Discord.Commands;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Umbreon.Preconditions
{
    public class RequireHierarchyAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            var currentUser = (context as SocketCommandContext)?.Guild.CurrentUser;
            if (value is SocketGuildUser guildUser)
            {
                return Task.FromResult(currentUser.Hierarchy > guildUser.Hierarchy
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("You don't have hierarchy over this user"));
            }

            return Task.FromResult(PreconditionResult.FromError("This error shouldn't exist"));
        }
    }
}
