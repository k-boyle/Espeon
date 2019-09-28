using Discord;
using Espeon.Commands;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    /*
     * Bug
     * Feature
     * Source
     */

    [Name("Bot Support")]
    [Description("Bot specific support")]
    public class Support : EspeonModuleBase
    {
        [Command("Bug")]
        [Name("Report Bug")]
        [Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.Support)]
        [Description("Report a bug, please be as precise as possible")]
        public async Task ReportBugAsync(
            [RequireSpecificLength(1000)]
            [Remainder]
            string bug)
        {
            var channel = Context.Client.GetChannel(463299724326469634) as IMessageChannel;

            await channel.SendMessageAsync($"{Context.Guild.Id}/{Context.Channel.Id}/{Context.User.Id}\n{bug}");

            await SendOkAsync(0);
        }

        [Command("Feature")]
        [Name("Feature Request")]
        [Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.Support)]
        [Description("Request a new feature to be added to the bot")]
        public async Task FeatureRequestAsync(
            [RequireSpecificLength(1000)]
            [Remainder]
            string feature)
        {
            var channel = Context.Client.GetChannel(463300066740797463) as IMessageChannel;

            await channel.SendMessageAsync($"{Context.Guild.Id}/{Context.Channel.Id}/{Context.User.Id}\n{feature}");

            await SendOkAsync(0);
        }

        [Command("Source")]
        [Name("Source Code")]
        [Description("Get the bots source code")]
        public Task GetSourceAsync()
            => SendMessageAsync("https://github.com/TheCasino/Espeon");
    }
}
