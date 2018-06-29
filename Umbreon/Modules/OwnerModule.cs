using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Umbreon.Activities;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;

namespace Umbreon.Modules
{
    [Group("cas")]
    [Name("Owner Commands")]
    [Summary("Super duper secret commands")]
    public class OwnerModule : UmbreonBase<GuildCommandContext>
    {
        [Command("playing")]
        [Name("Set Playing")]
        [Summary("Change what the bot is playing")]
        public async Task SetPlaying(
            [Name("Activity")]
            [Summary("Listening, Streaming, Playing, Watching")]
            ActivityType activity, [Remainder] string playing)
        {
            await Context.Client.SetActivityAsync(new Activity(playing, activity));
            await SendMessageAsync("Activity has been set");
        }
    }
}
