using Espeon.Commands;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    [Name("Emote Collecting")]
    [Group("ec")]
    [Description("Bootleg nitro that lets you use emotes not part of your guild")]
    public class Emotes : EspeonModuleBase
    {
        public Config Config { get; set; }

        [Command("toggle")]
        [Name("Toggle Emotes")]
        [Description("Enables/disables emotes for this guild")]
        [RequireElevation(ElevationLevel.Admin)]
        public Task ToggleEmotesAsync()
        {
            var guild = Context.CurrentGuild;

            guild.EmotesEnabled = !guild.EmotesEnabled;

            Context.GuildStore.Update(guild);

            return Task.WhenAll(SendOkAsync(0, guild.EmotesEnabled ? "enabled" : "disable"),
                Context.GuildStore.SaveChangesAsync());
        }

        [Command("capacity")]
        [Name("Emote Capacity")]
        [Description("View how many more emotes the bot can currently store")]
        public Task EmoteCapacityAsync()
        {
            var guilds = Config.EmoteGuilds.Select(x => Context.Client.GetGuild(x)).ToArray();
            var normal = guilds.Sum(x => x.Emotes.Count(y => !y.Animated));
            var animated = guilds.Sum(x => x.Emotes.Count(y => y.Animated));
            var length = guilds.Length;

            return SendOkAsync(0, normal, length * 50, animated, length * 50);
        }

        [Command("add")]
        [Name("Add Emote")]
        [Description("Adds a new emote to the bots collection")]
        public async Task AddEmoteAsync()
        {

        }
    }
}
