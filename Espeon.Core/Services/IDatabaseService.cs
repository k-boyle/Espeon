using System.Collections.Immutable;
using System.Threading.Tasks;
using Espeon.Core.Entities;

namespace Espeon.Core.Services
{
    public interface IDatabaseService
    {
        Task<T> GetEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity;
        Task<T> GetAndCacheEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity;
        Task WriteAsync<T>(string collection, T entity) where T : DatabaseEntity;
        Task<ImmutableArray<T>> GetCollectionAsync<T>(string collection) where T : DatabaseEntity;
    }
}
