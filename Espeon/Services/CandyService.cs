using System.Threading.Tasks;
using Espeon.Core.Attributes;
using Espeon.Core.Services;

namespace Espeon.Services
{
    [Service(typeof(ICandyService), true)]
    public class CandyService : ICandyService
    {
        public Task AddCandiesAsync(ulong id, int amount)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveCandiesAsync(ulong id, int amount)
        {
            throw new System.NotImplementedException();
        }

        public Task ClaimCandiesAsync(ulong id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> CanClaimCandiesAsync(ulong id)
        {
            throw new System.NotImplementedException();
        }
    }
}
