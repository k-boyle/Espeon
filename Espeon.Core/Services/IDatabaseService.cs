using Espeon.Core.Entities;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface IDatabaseService
    {
        Task<T> GetEntityAsync<T>(string collection, ulong id) where T : DatabaseEntity;
        Task WriteEntityAsync<T>(string collection, T entity) where T : DatabaseEntity;
        Task<ImmutableArray<T>> GetCollectionAsync<T>(string collection) where T : DatabaseEntity;
    }
}
