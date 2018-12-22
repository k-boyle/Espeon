using Espeon.Core.Attributes;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Deprecated
{
    [Service(typeof(IDatabaseService), ServiceLifetime.Singleton, false)]
    public class DatabaseService : IDatabaseService
    {
        private const string Dir = "./Database.db";

        [Inject] private readonly ITimerService _timer;

        private readonly ConcurrentDictionary<ulong, DatabaseEntity> _cache;

        private readonly SemaphoreSlim _getEntitySemaphore;
        private readonly SemaphoreSlim _writeEntitySemaphore;
        private readonly SemaphoreSlim _getAndCacheSemaphore;
        private readonly SemaphoreSlim _getCollectionSemaphore;

        public DatabaseService()
        {
            _cache = new ConcurrentDictionary<ulong, DatabaseEntity>();
            _getEntitySemaphore = new SemaphoreSlim(1, 1);
            _writeEntitySemaphore = new SemaphoreSlim(1, 1);
            _getAndCacheSemaphore = new SemaphoreSlim(1, 1);
            _getCollectionSemaphore = new SemaphoreSlim(1, 1);
        }

        private async Task<T> LoadEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity
        {
            await _getEntitySemaphore.WaitAsync();

            using (var db = new LiteDatabase(Dir))
            {
                var dbCollection = db.GetCollection<T>(collection);

                _getEntitySemaphore.Release();

                return dbCollection.FindOne(x => x.Id == id);
            }
        }

        public async Task WriteEntityAsync<T>(string collection, T entity) where T : DatabaseEntity
        {
            await _writeEntitySemaphore.WaitAsync();

            using (var db = new LiteDatabase(Dir))
            {
                var dbCollection = db.GetCollection<T>(collection);
                dbCollection.Upsert(entity);

                _writeEntitySemaphore.Release();
            }
        }

        public async Task<T> GetEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity
        {
            await _getAndCacheSemaphore.WaitAsync();

            if (_cache.TryGetValue(id, out var cached))
            {
                _getAndCacheSemaphore.Release();
                return (T) cached;
            }

            cached = await LoadEntityAsync<T>(collection, id);

            if (cached is null)
            {
                _getAndCacheSemaphore.Release();
                return null;
            }

            cached.WhenToRemove = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds();
            await WriteEntityAsync(collection, (T) cached);

            _cache[id] = cached;
            await _timer.EnqueueAsync(cached, RemoveAsync);

            _getAndCacheSemaphore.Release();
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
            await _getCollectionSemaphore.WaitAsync();

            using (var db = new LiteDatabase(Dir))
            {
                _getCollectionSemaphore.Release();
                return db.GetCollection<T>(collection).FindAll().ToImmutableArray();
            }
        }
    }
}
