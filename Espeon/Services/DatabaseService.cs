using Espeon.Core.Attributes;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(typeof(IDatabaseService), ServiceLifetime.Transient, true)]
    public class DatabaseService : IDatabaseService, IDisposable
    {
        private const string Dir = "./Database.db";
        private readonly LiteDatabase _database;
        private readonly SemaphoreSlim _semaphore;

        public DatabaseService(LiteDatabase database)
        {
            _database = database;
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<T> GetEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity
        {
            await _semaphore.WaitAsync();

            var dbCollection = _database.GetCollection<T>(collection);

            _semaphore.Release();

            return dbCollection.FindOne(x => x.Id == id);
        }

        public async Task WriteEntityAsync<T>(string collection, T entity) where T : DatabaseEntity
        {
            await _semaphore.WaitAsync();

            var dbCollection = _database.GetCollection<T>(collection);
            dbCollection.Upsert(entity);

            _semaphore.Release();
        }

        public async Task<ImmutableArray<T>> GetCollectionAsync<T>(string collection) where T : DatabaseEntity
        {
            await _semaphore.WaitAsync();

            var dbCollection = _database.GetCollection<T>(collection);

            _semaphore.Release();

            return dbCollection.FindAll().ToImmutableArray();
        }

        public void Dispose()
        {
            _database?.Dispose();
        }
    }
}
