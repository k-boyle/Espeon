using System.Threading.Tasks;
using Espeon.Commands.Interactive;

namespace Espeon.Commands
{
    public interface IGame : IReactionCallback
    {
        Task StartAsync();
        Task EndAsync();
    }
}
