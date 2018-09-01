using System.Threading.Tasks;

namespace Umbreon.Interfaces
{
    public interface IGame
    {
        Task StartAsync();
        Task EndAsync();
    }
}
