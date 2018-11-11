using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
        private readonly SemaphoreSlim _semaphore;

        public DatabaseService()
        {
           _cache = new ConcurrentDictionary<ulong, DatabaseEntity>(); 
           _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<T> GetEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity
        {
            await _semaphore.WaitAsync();

            using (var db = new LiteDatabase(Dir))
            {
                var dbCollection = db.GetCollection<T>(collection);

                _semaphore.Release();

                return dbCollection.FindOne(x => x.Id == id);
            }
        }

        public async Task WriteAsync<T>(string collection, T entity) where T : DatabaseEntity
        {
            await _semaphore.WaitAsync();

            using (var db = new LiteDatabase(Dir))
            {
                var dbCollection = db.GetCollection<T>(collection);
                dbCollection.Upsert(entity);

                _semaphore.Release();
            }            
        }

        public async Task<T> GetAndCacheEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity
        {
            await _semaphore.WaitAsync();

            if (_cache.TryGetValue(id, out var cached))
                return (T) cached;

            cached = await GetEntityAsync<T>(collection, id);

            if (cached is null)
                return null;

            cached.WhenToRemove = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds();
            await WriteAsync(collection, (T) cached);

            _cache[id] = cached;
            await _timer.EnqueueAsync(cached, RemoveAsync);

            _semaphore.Release();
            return (T) cached;
        }

        private Task RemoveAsync(string __, IRemovable removable)
        {
            var entity = removable as DatabaseEntity;
            //shouldn't nullref
            _cache.TryRemove(entity.Id, out _);

            return Task.CompletedTask;
        }

        public async Task<ImmutableArray<T>> GetCollectionAsync<T>(string collection) where T : DatabaseEntity
        {
            await _semaphore.WaitAsync();

            using (var db = new LiteDatabase(Dir))
            {
                _semaphore.Release();
                return db.GetCollection<T>(collection).FindAll().ToImmutableArray();
            }
        }
    }
}
