using System.Threading.Tasks;
using Espeon.Interactive;

namespace Espeon.Commands.Games
{
    public interface IGame : IReactionCallback
    {
        Task StartAsync();
        Task EndAsync();
    }
}
