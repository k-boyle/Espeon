using Discord;
using LiteDB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Entities.Guild;
using Umbreon.Helpers;
using Umbreon.Interfaces;

namespace Umbreon.Services
{
    [Service]
    public class DatabaseService : IRemoveableService
    {
        private readonly Random _random;
        private readonly TimerService _timer;
        private readonly DiscordSocketClient _client;
        private readonly LogService _log;

        private readonly ConcurrentDictionary<ulong, GuildObject> _guilds = new ConcurrentDictionary<ulong, GuildObject>();

        public DatabaseService() { }

        public DatabaseService(Random random, TimerService timer, DiscordSocketClient client, LogService log)
        {
            _random = random;
            _timer = timer;
            _client = client;
            _log = log;
        }

        public static Task Initialize()
        {
            var config = JObject.Parse(File.ReadAllText(ConstantsHelper.ConfigDir));
            ConstantsHelper.BotToken = $"{config["token"]}";
            ConstantsHelper.GiphyToken = $"{config["giphy"]}";
            return Task.CompletedTask;
        }

        private GuildObject LoadGuild(IGuild toLoad)
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                var guild = guilds.FindOne(x => x.GuildId == toLoad.Id) ?? NewGuild(toLoad);
                guild.When = DateTime.UtcNow + TimeSpan.FromDays(1);
                _guilds.TryAdd(toLoad.Id, guild);
                _timer.Update(guild);
                _log.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{toLoad.Name} has been loaded into cache");
                return guild;
            }
        }

        private GuildObject NewGuild(IGuild guild)
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                var newGuild = new GuildObject
                {
                    GuildId = guild.Id,
                    Identifier = _random.Next(),
                    Service = this
                };
                newGuild.WhiteListedUsers.Add(guild.OwnerId);
                guilds.Upsert(newGuild);
                _log.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{guild.Name} has been added to the database");
                return newGuild;
            }
        }

        public GuildObject TempLoad(IGuild guild)
        {
            if (_guilds.TryGetValue(guild.Id, out var found))
                return found;

            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                var guilds = db.GetCollection<GuildObject>("guilds");
                var foundGuild = guilds.FindOne(x => x.GuildId == guild.Id);
                if (!(foundGuild is null)) return foundGuild;
                foundGuild = new GuildObject
                {
                    GuildId = guild.Id,
                    Identifier = _random.Next(),
                    Service = this,
                };
                foundGuild.WhiteListedUsers.Add(guild.OwnerId);
                return foundGuild;
            }
        }

        public void UpdateGuild(GuildObject guild)
        {
            using (var db = new LiteDatabase(ConstantsHelper.DatabaseDir))
            {
                db.GetCollection<GuildObject>("guilds").Upsert(guild);
                _guilds[guild.GuildId] = guild;
            }
        } 

        public Task RemoveAsync(IRemoveable obj)
        {
            if (obj is GuildObject guild)
            {
                _guilds.TryRemove(guild.GuildId, out _);
            }

            return Task.CompletedTask;
        }

        public GuildObject GetGuild(ulong guildId)
            => GetGuild(_client.GetGuild(guildId));

        public GuildObject GetGuild(ICommandContext context)
            => GetGuild(context.Guild);

        private GuildObject GetGuild(IGuild guild)
            => _guilds.TryGetValue(guild.Id, out var found) ? found : LoadGuild(guild);
    }
}
