using System.Threading.Tasks;

namespace Espeon.Commands
{
    public interface IGame : IReactionCallback
    {
        Task StartAsync();
        Task EndAsync();
    }
}
