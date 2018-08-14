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

namespace Umbreon.Services
{
    [Service]
    public class DatabaseService
    {
        private readonly DiscordSocketClient _client;
        private readonly LogService _logs;
        private readonly ConcurrentDictionary<ulong, GuildObject> _guilds = new ConcurrentDictionary<ulong, GuildObject>();

        public DatabaseService(DiscordSocketClient client, LogService logs)
        {
            _client = client;
            _logs = logs;
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
                GuildId = guild.Id
            };
            guilds.Insert(newGuild);
            _guilds.TryAdd(guild.Id, newGuild);
            _logs.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{guild.Name} has been added to the database");
            return newGuild;
        }

        public GuildObject GetGuild(ICommandContext context)
            => GetGuild(context.Guild.Id);

        public GuildObject GetGuild(ulong guildId)
            => _guilds[guildId];

        public void UpdateGuild(GuildObject guild)
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                db.GetCollection<GuildObject>("guilds").Update(guild);
                _guilds[guild.GuildId] = guild;
            }
        }
    }
}
