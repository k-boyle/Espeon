using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.Preconditions
{
    public class RequireMusicAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetService<DatabaseService>();
            var guild = await database.GetObjectAsync<GuildObject>("guilds", context.Guild.Id);
            return guild.MusicUsers.Contains(context.User.Id)
                ? PreconditionResult.FromSuccess(command)
                : PreconditionResult.FromError(command, "You don't have permission to use this command");
        }
    }
}
