using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface ICandyService
    {
        Task AddCandiesAsync(ulong id, int amount);
        Task RemoveCandiesAsync(ulong id, int amount);
        Task ClaimCandiesAsync(ulong id);
        Task<int> GetCandiesAsync(ulong id);
        Task<bool> CanClaimCandiesAsync(ulong id);
    }
}
