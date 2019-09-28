using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface ICommandHandlingService
    {
        Task SetupCommandsAsync(CommandStore commandStore);
        Task ExecuteCommandAsync(SocketUser author, ISocketMessageChannel channel, string content, SocketUserMessage message);
    }
}
