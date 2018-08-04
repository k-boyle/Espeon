using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Umbreon.Preconditions
{
    public class RequireSameChannelAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var botChannel = (await context.Guild.GetCurrentUserAsync()).VoiceChannel;
            if(botChannel is null) return PreconditionResult.FromSuccess();
            return (context.User as IGuildUser).VoiceChannel.Id == botChannel.Id
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("You must be in the same channel as the bot to use this command");
        }
    }
}
