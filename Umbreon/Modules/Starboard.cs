using Discord;
using Discord.Commands;
using Discord.Net.Helpers;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;
using Umbreon.TypeReaders;

namespace Umbreon.Modules
{
    [Group("star")]
    [Name("Starboard")]
    [Summary("hi")]
    public class Starboard : ServerSettingsBase<GuildCommandContext>
    {
        [Command("Channel")]
        [Name("Set Channel")]
        [Summary("Set the starboard channel for this guild")]
        [@Remarks("Leave blank to disable starboard")]
        [Usage("star channel #starboard")]
        [RequireRole(SpecialRole.Admin)]
        public async Task SetChannel(
            [Name("Star Channel")]
            [Summary("The channel you want starboard to be, leave blank to disable starboard")]
            [Remainder] SocketTextChannel starChannel = null)
        {
            if (starChannel is null)
            {
                CurrentGuild.Starboard.Enabled = false;
                await SendMessageAsync("Starboard has been disabled");
                return;
            }

            CurrentGuild.Starboard.Enabled = true;
            CurrentGuild.Starboard.ChannelId = starChannel.Id;
            await SendMessageAsync("Starboard has been enabled");
        }

        [Command("Limit")]
        [Name("Star Limit")]
        [Summary("Set the star limit for starboard")]
        [Usage("star limit 3")]
        [RequireRole(SpecialRole.Admin)]
        [RequireStarboard]
        public async Task StarLimit(
            [Name("Star Limit")]
            [Summary("How many stars are required")]
            [OverrideTypeReader(typeof(StarLimitTypeReader))] int starLimit)
        {
            CurrentGuild.Starboard.StarLimit = starLimit;
            await SendMessageAsync("Star limit has been updated");
        }

        [Command("Leaderboard")]
        [Name("Leaderboard")]
        [Summary("See the top starred messages")]
        [Usage("star leaderboard")]
        public async Task LeaderBoard()
        {
            var starboard = CurrentGuild.Starboard;
            var ordered = starboard.StarredMessages.OrderByDescending(x => x.StarCount);
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = Context.User.GetDisplayName(),
                    IconUrl = Context.User.GetAvatarOrDefaultUrl()
                },
                Color = Color.Gold,
                Title = "Starboard Leaderboard"
            };
            var count = 0;
            foreach (var item in ordered)
            {
                
                embed.AddField(f => { f.Name = $""; });
            }
        }
    }
}
