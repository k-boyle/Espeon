using Discord.WebSocket;
using Espeon.Commands.Checks;
using Espeon.Enums;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
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
    }
}
