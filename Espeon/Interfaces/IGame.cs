using System.Threading.Tasks;

namespace Espeon.Interfaces
{
    public interface IGame
    {
        Task StartAsync();
        Task EndAsync();
    }
}
