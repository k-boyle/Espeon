using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Espeon.Core.Attributes;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using LiteDB;

namespace Espeon.Services
{
    [Service(typeof(IDatabaseService), true)]
    public class DatabaseService : IDatabaseService
    {
        private const string Dir = "./Database.db";

        [Inject] private readonly ITimerService _timer;

        private readonly ConcurrentDictionary<ulong, DatabaseEntity> _cache;

        public DatabaseService()
        {
           _cache = new ConcurrentDictionary<ulong, DatabaseEntity>(); 
        }

        public Task<T> GetEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity
        {
            using (var db = new LiteDatabase(Dir))
            {
                var dbCollection = db.GetCollection<T>(collection);
                return Task.FromResult(dbCollection.FindOne(x => x.Id == id));
            }
        }

        public Task WriteAsync<T>(string collection, T entity) where T : DatabaseEntity
        {
            using (var db = new LiteDatabase(Dir))
            {
                var dbCollection = db.GetCollection<T>(collection);
                dbCollection.Upsert(entity);
                return Task.CompletedTask;
            }
        }

        public async Task<T> GetAndCacheEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity
        {
            if (_cache.TryGetValue(id, out var cached))
                return (T) cached;

            cached = await GetEntityAsync<T>(collection, id);

            if (cached is null)
                return null;

            cached.WhenToRemove = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds();
            await WriteAsync(collection, (T) cached);

            _cache[id] = cached;
            await _timer.EnqueueAsync(cached, RemoveAsync);

            return (T) cached;
        }

        private Task RemoveAsync(string __, IRemovable removable)
        {
            var entity = removable as DatabaseEntity;
            //shouldn't nullref
            _cache.TryRemove(entity.Id, out _);

            return Task.CompletedTask;
        }
    }
}
