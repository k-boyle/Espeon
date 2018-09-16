using Discord;
using LiteDB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Entities;
using Umbreon.Helpers;
using Umbreon.Interfaces;

namespace Umbreon.Services
{
    [Service]
    public class DatabaseService : IRemoveableService
    {
        private const string DatabaseDir = "./Database.db";
        private const string ConfigDir = "./config.json";
        private readonly ConcurrentDictionary<ulong, BaseObject> _cache = new ConcurrentDictionary<ulong, BaseObject>();

        private readonly Random _random;
        private readonly TimerService _timer;
        private readonly LogService _log;

        public DatabaseService(Random random, TimerService timer, LogService log)
        {
            _random = random;
            _timer = timer;
            _log = log;
        }

        public DatabaseService() { }

        public static Task InitialiseAsync()
        {
            var config = JObject.Parse(File.ReadAllText(ConfigDir));
            ConstantsHelper.BotToken = $"{config["token"]}";
            ConstantsHelper.GiphyToken = $"{config["giphy"]}";
            ConstantsHelper.PokemonLimit = int.Parse($"{config["pokemonlimit"]}");
            return Task.CompletedTask;
        }

        private async Task<T> LoadObject<T>(string name, ulong id) where T : BaseObject, new()
        {
            using (var db = new LiteDatabase(DatabaseDir))
            {
                var collection = db.GetCollection<T>(name);
                var found = collection.FindOne(x => x.Id == id) ?? NewObject<T>(id);
                found.When = DateTime.UtcNow.AddDays(1);
                collection.Upsert(found);
                await _timer.UpdateAsync(found);
                _cache.TryAdd(found.Id, found);
                _log.NewLogEvent(LogSeverity.Info, LogSource.Database, $"{found.Id}:{found.GetType()} has been loaded into the cache");
                return found;
            }
        }

        private T NewObject<T>(ulong id) where T : BaseObject, new()
        {
            var newObj = new T
            {
                Id = id,
                Identifier = _random.Next()
            };

            var returnObj = (T)Activator.CreateInstance(typeof(T), newObj, this);
            return returnObj;
        }

        public T TempLoad<T>(string name, ulong id) where T : BaseObject, new()
        {
            if (_cache.TryGetValue(id, out var found))
                return (T)found;

            using (var db = new LiteDatabase(DatabaseDir))
            {
                var collection = db.GetCollection<T>(name);
                found = collection.FindOne(x => x.Id == id) ?? NewObject<T>(id);
                collection.Upsert((T)found);
                return (T)found;
            }
        }

        public void UpdateObject<T>(string name, T obj) where T : BaseObject
        {
            using (var db = new LiteDatabase(DatabaseDir))
            {
                db.GetCollection<T>(name).Upsert(obj);
                _cache[obj.Id] = obj;
            }
        }

        public async Task<T> GetObjectAsync<T>(string name, ulong id) where T : BaseObject, new()
            => _cache.TryGetValue(id, out var found) ? (T)found : await LoadObject<T>(name, id);

        public static IEnumerable<T> GrabAllData<T>(string name) where T : BaseObject
        {
            using (var db = new LiteDatabase(DatabaseDir))
            {
                return db.GetCollection<T>(name).FindAll();
            }
        }

        public Task RemoveAsync(IRemoveable obj)
        {
            if (obj is BaseObject baseObj)
                _cache.TryRemove(baseObj.Id, out _);
            return Task.CompletedTask;
        }
    }
}
