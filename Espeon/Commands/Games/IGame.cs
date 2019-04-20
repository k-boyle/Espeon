using System.Threading.Tasks;

namespace Espeon.Commands
{
    public interface IGame : IReactionCallback
    {
        Task<bool> StartAsync();
        Task EndAsync();
    }
}
