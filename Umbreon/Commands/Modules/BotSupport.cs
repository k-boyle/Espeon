using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Extensions;

namespace Umbreon.Commands.Modules
{
    [Name("Bot Support")]
    [Summary("Commands to give feedback/support for the bot")]
    public class BotSupport : UmbreonBase
    {
        [Command("Bug")]
        [Name("Bug Report")]
        [Summary("Submit a bug report. Please be as informative as possible")]
        [Usage("bug Umbreon is 2 cwl")]
        public Task BugReport(
            [Name("Report")]
            [Summary("The bug, as descriptive as possible please")]
            [Remainder] string report)
            => (Context.Client.GetChannel(463299724326469634) as SocketTextChannel)?.SendMessageAsync($"{DateTime.UtcNow.TimeOfDay} : {Context.User.GetDisplayName()} : {Context.Guild.Name} : {Context.Channel.Name}({Context.Channel.Id}) - {report}");

        [Command("Feature")]
        [Name("Feature Request")]
        [Summary("Submit a feature request")]
        [Usage("feature make umbreon cwler")]
        public Task FeatureReq(
            [Name("Request")]
            [Summary("The feature that you want")]
            [Remainder] string feature)
            => (Context.Client.GetChannel(463300066740797463) as SocketTextChannel)?.SendMessageAsync($"{DateTime.UtcNow.TimeOfDay} : {Context.User.GetDisplayName()} : {Context.Guild.Name} : {Context.Channel.Name}({Context.Channel.Id}) - {feature}");

        [Command("Source")]
        [Name("Bot Source")]
        [Summary("The source code for the bot")]
        [Usage("source")]
        public Task GetSource()
            => SendMessageAsync("https://github.com/purpledank/Umbreon");
    }
}
