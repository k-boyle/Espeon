using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Core;
using Umbreon.Core.Models.Database;
using Umbreon.Helpers;

namespace Umbreon.Services
{
    public class DatabaseService
    {
        private readonly DiscordSocketClient _client;
        private readonly LogService _logs;
        private readonly Dictionary<ulong, GuildObject> _guilds = new Dictionary<ulong, GuildObject>(); // TODO change to ConcurrentDictionary

        public DatabaseService(DiscordSocketClient client, LogService logs )
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
                if (config is null || string.IsNullOrEmpty(config.BotToken))
                {
                    configCol.EnsureIndex("0");
                    Console.Write("Bot token was not found please input it: ");
                    var token = Console.ReadLine();
                    configCol.Upsert(new BotConfig
                    {
                        BotToken = token,
                        Index = 0
                    });
                    ConstantsHelper.BotToken = token;
                    _logs.NewLogEvent(LogSeverity.Info, LogSource.Database, "Config has been added to the database");
                    return Task.CompletedTask;
                }

                ConstantsHelper.BotToken = config.BotToken;
            }
            return Task.CompletedTask;
        }

        public void LoadGuilds()
        {
            _guilds.Clear();
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                foreach (var guild in _client.Guilds)
                {
                    var g = guilds.FindOne(x => x.GuildId == guild.Id);
                    if (g is null)
                    {
                        NewGuild(guilds, guild);
                    }
                    else
                    {
                        _guilds.Add(g.GuildId, g);
                        _logs.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{guild.Name} has been loaded");
                    }
                }
            }
        }

        public void NewGuild(SocketGuild guild)
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                NewGuild(guilds, guild);
            }
        }

        public void NewGuild(LiteCollection<GuildObject> guilds, SocketGuild guild)
        {
            var newGuild = new GuildObject
            {
                GuildId = guild.Id
            };
            guilds.Insert(newGuild);
            _guilds.Add(guild.Id, newGuild);
            _logs.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{guild.Name} has been added to the database");
        }

        public GuildObject GetGuild(ICommandContext context)
            => GetGuild(context.Guild.Id);

        public GuildObject GetGuild(ulong guildId)
        {
            return _guilds[guildId];
        }

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
