using LiteDB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Umbreon.Attributes;
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

        public DatabaseService(Random random, TimerService timer)
        {
            _random = random;
            _timer = timer;
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

        private async Task<T> AddToCacheAsync<T>(string name, ulong id) where T : BaseObject, new()
        {
            var obj = LoadFromDatabase<T>(name, id);
            obj.When = DateTime.UtcNow.AddDays(1);

            var instance = CreateInstance(obj);
            _cache.TryAdd(instance.Id, instance);
            await _timer.UpdateAsync(instance);
            return instance;
        }

        private T LoadFromDatabase<T>(string name, ulong id) where T : BaseObject, new()
        {
            using (var db = new LiteDatabase(DatabaseDir))
            {
                var collection = db.GetCollection<T>(name);
                var loaded = collection.FindOne(x => x.Id == id);

                if (!(loaded is null)) return loaded;
                loaded = NewObject<T>(id);
                collection.Upsert(loaded);

                return loaded;
            }
        }

        private T NewObject<T>(ulong id) where T : BaseObject, new()
            => new T {Id = id, Identifier = _random.Next() };

        private T CreateInstance<T>(T obj) where T : BaseObject
            => (T) Activator.CreateInstance(typeof(T), obj, this);

        public T TempLoad<T>(string name, ulong id) where T : BaseObject, new()
        {
            if (_cache.TryGetValue(id, out var found))
                return (T)found;

            return LoadFromDatabase<T>(name, id);
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
            => _cache.TryGetValue(id, out var found) ? (T) found : await AddToCacheAsync<T>(name, id);

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
