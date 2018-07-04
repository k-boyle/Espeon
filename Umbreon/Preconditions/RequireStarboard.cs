using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Services;

namespace Umbreon.Preconditions
{
    public class RequireStarboard : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetService<DatabaseService>();
            var guild = database.GetGuild(context);
            var starboard = guild.Starboard;
            return starboard.Enabled ? Task.FromResult(PreconditionResult.FromSuccess()) : Task.FromResult(
                PreconditionResult.FromError("Starboard is not enabled on this server"));
        }
    }
}
