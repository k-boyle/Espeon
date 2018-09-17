using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Commands.Contexts;
using Espeon.Commands.ModuleBases;
using Espeon.Commands.Preconditions;
using Espeon.Commands.TypeReaders;
using Espeon.Extensions;
using Colour = Discord.Color;

namespace Espeon.Commands.Modules
{
    [Summary("Music commands")]
    [RequireMusic]
    public class Music : MusicModuleBase<EspeonContext>
    {
        [Command("join")]
        [Summary("Gets the bot to join your voice channel")]
        [Usage("join")]
        [Name("Join Channel")]
        public async Task JoinChannel()
        {
            if (Context.User.VoiceChannel is null)
            {
                await SendMessageAsync("You need to be in a voice channel to invoke this command");
                return;
            }

            await Music.JoinAsync(Context);
        }

        [Command("play")]
        [Summary("Plays the passed song")]
        [Usage("play https://www.youtube.com/watch?v=y6120QOlsfU")]
        [Name("Play Song")]
        [RequireSameChannel]
        public async Task PlayMusic(
            [Name("Song")]
            [Summary("The song you want to play")]
            [Remainder] string toSearch)
        {
            var track = await Music.GetTrackAsync(toSearch);
            if (track is null)
            {
                await SendMessageAsync("No track found");
                return;
            }

            var res = await Music.PlayTrackAsync(Context, track);
            var embed = new EmbedBuilder
            {
                Title = res ? $"{track.Title} added to queue" : $"Now playing {track.Title}",
                Color = Colour.Red
            };
            await SendMessageAsync(string.Empty, embed: embed.Build());
        }

        [Command("leave")]
        [Summary("Leave the current voice channel")]
        [Usage("leave")]
        [Name("Leave Channel")]
        [RequireSameChannel]
        public async Task LeaveChannel()
        {
            if (Context.Guild.CurrentUser.VoiceChannel is null)
            {
                await SendMessageAsync("Bot is not in a voice channel");
                return;
            }
            await Music.LeaveAsync(Context);
        }

        [Command("volume")]
        [Summary("Set the volume")]
        [Usage("volume 69")]
        [Name("Set Volume")]
        [RequireSameChannel]
        public async Task SetVolume(
            [Name("Volume")]
            [Summary("The volume you want to set")]
            [OverrideTypeReader(typeof(VolumeTypeReader))] uint volume)
        {
            await Music.SetVolumeAsync(Context, volume);
            await SendMessageAsync($"Volume has been set to: {Math.Floor(volume / 1.5)}");
        }

        [Command("pause")]
        [Summary("Pauses the current song")]
        [Usage("pause")]
        [Name("Pause Song")]
        [RequireSameChannel]
        public async Task Pause()
        {
            await Music.PauseAsync(Context);
        }

        [Command("resume")]
        [Summary("Resumes a paused song")]
        [Usage("resume")]
        [Name("Resume Song")]
        [RequireSameChannel]
        public async Task Resume()
        {
            await Music.ResumeAsync(Context);
        }

        [Command("skip")]
        [Summary("Skips the current song")]
        [Usage("skip")]
        [Name("Skip Song")]
        [RequireSameChannel]
        public async Task Skip()
        {
            await Music.SkipSongAsync(Context);
        }

        [Command("approve")]
        [Summary("Approve someone to use music commands")]
        [Usage("approve Espeon")]
        [Name("Approve User")]
        [RequireOwner]
        public async Task Approve(
            [Name("User")]
            [Summary("The user you want to approve")]
            [Remainder] SocketGuildUser user)
        {
            var guild = await CurrentGuild;
            guild.MusicUsers.Add(user.Id);
            await SendMessageAsync($"{user.GetDisplayName()} has been approved");
            Database.UpdateObject("guilds", guild);
        }

        [Command("unapprove")]
        [Summary("Unapprove a user for music commands")]
        [Usage("unapprove Espeon")]
        [Name("Unapprove User")]
        [RequireOwner]
        public async Task Unapprove(
            [Name("User")]
            [Summary("The user you want to unapprove")]
            [Remainder] SocketGuildUser user)
        {
            var guild = await CurrentGuild;
            guild.MusicUsers.Remove(user.Id);
            await SendMessageAsync($"{user.GetDisplayName()} has been unapproved");
            Database.UpdateObject("guilds", guild);
        }

        [Command("queue")]
        [Summary("View the current song queue")]
        [Usage("queue")]
        [Name("Music Queue")]
        public async Task ViewQueue()
        {
            var currentQueue = Music.GetGuild(Context).Queue;
            await SendMessageAsync(string.Empty, embed: new EmbedBuilder
            {
                Title = "Current Queue",
                Color = Colour.Red,
                Description = string.Join("\n", currentQueue.Select(x => x.Title))
            }.Build());
        }
    }
}
