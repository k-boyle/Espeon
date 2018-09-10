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
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetService<DatabaseService>();
            var guild = database.GetObject<GuildObject>("guilds", context.Guild.Id);
            return guild.MusicUsers.Contains(context.User.Id)
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError(new FailedResult("You do not have the permissions to use this command", false, CommandError.UnmetPrecondition)));
        }
    }
}
