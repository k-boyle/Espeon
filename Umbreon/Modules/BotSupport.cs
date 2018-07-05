﻿using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Net.Helpers;
using Umbreon.Attributes;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;

namespace Umbreon.Modules
{
    [Group("bot")]
    [Name("Bot Support")]
    [Summary("Commands to give feedback/support for the bot")]
    public class BotSupport : UmbreonBase<GuildCommandContext>
    {
        [Command("Bug")]
        [Name("Bug Report")]
        [Summary("Submit a bug report. Please be as informative as possible")]
        [Usage("bot bug Umbreon is 2 cwl")]
        public async Task BugReport(
            [Name("Report")]
            [Summary("The bug, as descriptive as possible please")]
            [Remainder] string report)
            => await (Context.Client.GetChannel(463299724326469634) as SocketTextChannel).SendMessageAsync($"{DateTime.UtcNow.TimeOfDay} : {Context.User.GetDisplayName()} : {Context.Guild.Name} : {Context.Channel.Name}({Context.Channel.Id}) - {report}");

        [Command("Feature")]
        [Name("Feature Request")]
        [Summary("Submit a feature request")]
        [Usage("bot feature make umbreon cwler")]
        public async Task FeatureReq(
            [Name("Request")]
            [Summary("The feature that you want")]
            [Remainder] string feature)
            => await (Context.Client.GetChannel(463300066740797463) as SocketTextChannel).SendMessageAsync($"{DateTime.UtcNow.TimeOfDay} : {Context.User.GetDisplayName()} : {Context.Guild.Name} : {Context.Channel.Name}({Context.Channel.Id}) - {feature}");

        [Command("Source")]
        [Name("Bot Source")]
        [Summary("The source code for the bot")]
        [Usage("bot source")]
        public async Task GetSource()
            => await SendMessageAsync("https://github.com/purpledank/Umbreon");
    }
}