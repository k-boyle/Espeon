using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SharpLink;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core.Entities;
using Colour = Discord.Color;

namespace Umbreon.Services
{
    [Service]
    public class MusicService
    {
        private readonly DiscordSocketClient _client;
        private readonly LogService _log;
        private readonly MessageService _message;

        private LavalinkManager _lavalinkManager;

        private ConcurrentDictionary<ulong, LavalinkObject> _lavaCache = new ConcurrentDictionary<ulong, LavalinkObject>();
        
        public MusicService(DiscordSocketClient client, LogService log, MessageService message)
        {
            _client = client;
            _log = log;
            _message = message;
        }

        public async Task InitialiseAsync()
        {
            _lavalinkManager = new LavalinkManager(_client, new LavalinkManagerConfig
            {
                RESTHost = "localhost",
                RESTPort = 2333,
                WebSocketHost = "localhost",
                WebSocketPort = 80,
                Authorization = "casinobestpass",
                TotalShards = 1,
                LogSeverity = LogSeverity.Verbose
            });
            _lavalinkManager.Log += _log.LogEvent;
            await _lavalinkManager.StartAsync();
            _lavalinkManager.TrackEnd += TrackFinishedAsync;
        }

        public LavalinkObject GetGuild(ICommandContext context)
            => _lavaCache.TryGetValue(context.Guild.Id, out var found) ? found : null;

        public async Task JoinAsync(ICommandContext context)
        {
            if (_lavaCache.ContainsKey(context.Guild.Id))
                await _lavalinkManager.LeaveAsync(context.Guild.Id);
            var player = await _lavalinkManager.JoinAsync((context.User as IGuildUser).VoiceChannel);
            var newObj = new LavalinkObject
            {
                ChannelId = context.Channel.Id,
                IsPaused = false,
                Player = player,
                UserId = context.User.Id,
                Queue = new ConcurrentQueue<LavalinkTrack>()
            };
            if (!_lavaCache.TryAdd(context.Guild.Id, newObj))
            {
                _lavaCache[context.Guild.Id] = new LavalinkObject
                {
                    ChannelId = context.Channel.Id,
                    IsPaused = false,
                    Player = player,
                    Queue = _lavaCache[context.Guild.Id].Queue,
                    UserId = context.User.Id
                };
            }
        }

        public async Task<bool> PlayTrackAsync(ICommandContext context, LavalinkTrack track)
        {
            if (!_lavaCache.ContainsKey(context.Guild.Id))
                await JoinAsync(context);
            var currentGuild = _lavaCache[context.Guild.Id];
            var player = currentGuild.Player;
            _lavaCache[context.Guild.Id].Queue.Enqueue(track);
            if (!player.Playing && !currentGuild.IsPaused)
                await player.PlayAsync(track);
            return _lavaCache[context.Guild.Id].Queue.Count > 1;
        }

        public Task<LavalinkTrack> GetTrackAsync(string toSearch)
            => _lavalinkManager.GetTrackAsync($"ytsearch:{toSearch}");

        private async Task TrackFinishedAsync(LavalinkPlayer player, LavalinkTrack __, string reason)
        {
            if (reason == "REPLACED") return;
            var guildId = player.VoiceChannel.GuildId;
            _lavaCache[guildId].Queue.TryDequeue(out _);
            if (_lavaCache[guildId].Queue.TryPeek(out var track))
            {
                await player.PlayAsync(track);
                var channel = _client.GetChannel(_lavaCache[guildId].ChannelId) as SocketTextChannel;
                var embed = new EmbedBuilder
                {
                    Title = $"Now playing {track.Title}",
                    Color = Colour.Red
                };
                await _message.NewMessageAsync(_lavaCache[guildId].UserId, 0, channel.Id, string.Empty, embed: embed.Build());
            }
            else
            {
                await player.StopAsync();
            }
        }

        public async Task LeaveAsync(ICommandContext context)
        {
            if (!_lavaCache.ContainsKey(context.Guild.Id)) return;
            await _lavalinkManager.LeaveAsync(context.Guild.Id);
            _lavaCache.TryRemove(context.Guild.Id, out _);
        }

        public async Task SetVolumeAsync(ICommandContext context, uint volume)
        {
            if (_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild))
                await currentGuild.Player.SetVolumeAsync(volume);
        }

        public async Task PauseAsync(ICommandContext context)
        {
            if (!_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild)) return;
            if (!currentGuild.IsPaused)
            {
                await currentGuild.Player.PauseAsync();
                _lavaCache[context.Guild.Id] = new LavalinkObject{
                    Player = currentGuild.Player,
                    IsPaused = true,
                    ChannelId = currentGuild.ChannelId,
                    UserId = currentGuild.UserId,
                    Queue = currentGuild.Queue
                };
            }
        }

        public async Task ResumeAsync(ICommandContext context)
        {
            if (!_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild)) return;
            if (currentGuild.IsPaused)
            {
                await currentGuild.Player.ResumeAsync();
                _lavaCache[context.Guild.Id] = new LavalinkObject
                {
                    Player = currentGuild.Player,
                    IsPaused = false,
                    ChannelId = currentGuild.ChannelId,
                    UserId = currentGuild.UserId,
                    Queue = currentGuild.Queue
                };
            }
        }

        public async Task SkipSongAsync(ICommandContext context)
        {
            if (!_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild)) return;
            currentGuild.Queue.TryDequeue(out _);
            if (currentGuild.Queue.TryPeek(out var track))
            {
                await currentGuild.Player.PlayAsync(track);
                var embed = new EmbedBuilder
                {
                    Title = $"Now playing {track.Title}",
                    Color = Colour.Red
                };
                await _message.NewMessageAsync(currentGuild.UserId, 0, currentGuild.ChannelId, string.Empty,
                    embed: embed.Build()); 
                _lavaCache[context.Guild.Id] = currentGuild;
            }
            else
            {
                await currentGuild.Player.StopAsync();
            }
        }
    }
}
