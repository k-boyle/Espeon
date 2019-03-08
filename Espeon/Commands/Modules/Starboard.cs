using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    /*
     * Star
     * View
     * Stats
     * Random
     */

    [Name("Starboard")]
    [Group("Star")]
    public class Starboard : EspeonBase
    {
        public Random Random { get; set; }

        [Command("enable")]
        [Name("Enable Starboard")]
        [RequireElevation(ElevationLevel.Admin)]
        public async Task EnableStarboardAsync([Remainder] SocketTextChannel channel)
        {
            var guild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            guild.StarboardChannelId = channel.Id;

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(0);
        }

        [Command("disable")]
        [Name("Disable Starboard")]
        [RequireElevation(ElevationLevel.Admin)]
        public async Task DisableStarboardAsync()
        {
            var guild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            guild.StarboardChannelId = 0;

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(0);
        }

        [Command("limit")]
        [Name("Set Starboard Limit")]
        [RequireElevation(ElevationLevel.Admin)]
        public async Task SetStarboardLimitAsync([RequireRange(0)] int limit)
        {
            var guild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            guild.StarLimit = limit;

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(0);
        }

        [Command("random")]
        [Name("Random Star")]
        public async Task ViewRandomStarAsync()
        {
            var guild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.StarredMessages);

            if(guild.StarredMessages.Count == 0)
            {
                await SendNotOkAsync(0);
                return;
            }

            var randomStar = guild.StarredMessages[Random.Next(guild.StarredMessages.Count)];

            var user = await Context.Guild.GetGuildUserAsync(randomStar.AuthorId);

            var starMessage = Utilities.BuildStarMessage(user, randomStar.Content, randomStar.ImageUrl);

            var m = string.Concat(
                $"{Utilities.Star}" ,
                $"**{randomStar.ReactionUsers.Count}** - ",
                $"{user.GetDisplayName()} in <#",
                $"{randomStar.ChannelId}>");

            await SendMessageAsync(m, starMessage);
        }
    }
}
