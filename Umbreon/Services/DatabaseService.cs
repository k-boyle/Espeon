using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Models.Database;
using Umbreon.Core.Models.Database.Guilds;
using Umbreon.Helpers;
using Umbreon.Interfaces;

namespace Umbreon.Services
{
    [Service]
    public class DatabaseService : IRemoveableService
    {
        private readonly DiscordSocketClient _client;
        private readonly LogService _logs;
        private readonly Random _random;
        private readonly TimerService _timer;

        private readonly ConcurrentDictionary<ulong, GuildObject> _guilds = new ConcurrentDictionary<ulong, GuildObject>();

        public DatabaseService(DiscordSocketClient client, LogService logs, Random random, TimerService timer)
        {
            _client = client;
            _logs = logs;
            _random = random;
            _timer = timer;
        }

        public Task Initialize()
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var configCol = db.GetCollection<BotConfig>("config");
                var config = configCol.FindAll().FirstOrDefault();
                if (config is null || string.IsNullOrEmpty(config.BotToken) || string.IsNullOrEmpty(config.Giphy))
                {
                    configCol.EnsureIndex("0");
                    var newConfig = new BotConfig
                    {
                        Index = 0,
                        BotToken = config?.BotToken,
                        Giphy = config?.Giphy
                    };
                    if(string.IsNullOrEmpty(newConfig.BotToken))
                    {
                        Console.Write("Bot token was not found please input it: ");
                        var token = Console.ReadLine();
                        newConfig.BotToken = token;
                        ConstantsHelper.BotToken = token;
                    }
                    if (string.IsNullOrEmpty(newConfig.Giphy))
                    {
                        Console.Write("Please input your Giphy API key: ");
                        var giphy = Console.ReadLine();
                        newConfig.Giphy = giphy;
                        ConstantsHelper.GiphyToken = giphy;
                    }

                    configCol.Upsert(newConfig);
                    _logs.NewLogEvent(LogSeverity.Info, LogSource.Database, "Config has been added to the database");
                    return Task.CompletedTask;
                }
                ConstantsHelper.BotToken = config.BotToken;
            }
            return Task.CompletedTask;
        }

        public Task LoadGuilds()
        {
            _guilds.Clear();
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                foreach (var guild in _client.Guilds)
                {
                    var g = guilds.FindOne(x => x.GuildId == guild.Id) ?? NewGuild(guilds, guild);
                    _timer.Enqueue(g);
                    _guilds.TryAdd(g.GuildId, g);
                    _logs.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{guild.Name} has been loaded");
                }
            }
            return Task.CompletedTask;
        }

        public void NewGuild(SocketGuild guild)
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                NewGuild(guilds, guild);
            }
        }

        private GuildObject NewGuild(LiteCollection<GuildObject> guilds, IGuild guild)
        {
            if (!(guilds.FindOne(x => x.GuildId == guild.Id) is null)) return null;
            var newGuild = new GuildObject
            {
                GuildId = guild.Id,
                Service = this,
                When = TimeSpan.FromDays(1),
                Identifier = _random.Next()
            };
            guilds.Upsert(newGuild);
            _guilds.TryAdd(guild.Id, newGuild);
            _logs.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{guild.Name} has been added to the database");
            return newGuild;
        }

        public GuildObject GetGuild(ICommandContext context)
            => GetGuild(context.Guild.Id);

        public GuildObject GetGuild(ulong guildId)
            => _guilds.TryGetValue(guildId, out var guild) ? guild : LoadGuild(guildId);

        private GuildObject LoadGuild(ulong guildId)
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                var guild = guilds.FindOne(x => x.GuildId == guildId);
                guild.When = TimeSpan.FromDays(1);
                UpdateGuild(guild);
                _guilds.TryAdd(guild.GuildId, guild);
                return guild;
            }
        }

        public void UpdateGuild(GuildObject guild)
        {
            _timer.Update(guild);

            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                db.GetCollection<GuildObject>("guilds").Upsert(guild);
                _guilds[guild.GuildId] = guild;
            }
        }

        public Task Remove(IRemoveable obj)
        {
            if (obj is GuildObject guild)
            {
                _guilds.TryRemove(guild.GuildId, out _);
            }

            return Task.CompletedTask;
        }
    }
}
