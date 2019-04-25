using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;
using Casino.Common.Discord.Net;

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
    [Description("Display messages in a hall of fame")]
    public class Starboard : EspeonBase
    {
        public Random Random { get; set; }

        [Command("enable")]
        [Name("Enable Starboard")]
        [RequireElevation(ElevationLevel.Admin)]
        [Description("Enables starboard settings the specified channel as the star channel")]
        public async Task EnableStarboardAsync([Remainder] SocketTextChannel channel)
        {
            var guild = Context.CurrentGuild;
            guild.StarboardChannelId = channel.Id;
            Context.GuildStore.Update(guild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("disable")]
        [Name("Disable Starboard")]
        [RequireElevation(ElevationLevel.Admin)]
        [Description("Disables starboard for this guild")]
        public async Task DisableStarboardAsync()
        {
            var guild = Context.CurrentGuild;
            guild.StarboardChannelId = 0;
            Context.GuildStore.Update(guild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("limit")]
        [Name("Set Starboard Limit")]
        [RequireElevation(ElevationLevel.Admin)]
        [Description("Change the number of stars needed for a message to be added to the starboard")]
        public async Task SetStarboardLimitAsync([RequireRange(1)] int limit)
        {
            var guild = Context.CurrentGuild;
            guild.StarLimit = limit;
            Context.GuildStore.Update(guild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("random")]
        [Name("Random Star")]
        [Description("Get a random message from the starboard")]
        public async Task ViewRandomStarAsync()
        {
            var guild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.StarredMessages);

            if(guild.StarredMessages.Count == 0)
            {
                await SendNotOkAsync(0);
                return;
            }

            var randomStar = guild.StarredMessages[Random.Next(guild.StarredMessages.Count)];

            var user = await Context.Guild.GetOrFetchUserAsync(randomStar.AuthorId)
                ?? await Context.Client.GetOrFetchUserAsync(randomStar.AuthorId);

            var jump = Utilities.BuildJumpUrl(Context.Guild.Id, randomStar.ChannelId, randomStar.Id);

            var starMessage = Utilities.BuildStarMessage(user, randomStar.Content, jump, randomStar.ImageUrl);

            var m = string.Concat(
                $"{Utilities.Star}" ,
                $"**{randomStar.ReactionUsers.Count}** - ",
                $"{(user as IGuildUser)?.GetDisplayName() ?? user.Username} in <#",
                $"{randomStar.ChannelId}>");

            await SendMessageAsync(m, starMessage);
        }
    }
}
