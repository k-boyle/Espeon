using Discord.WebSocket;
using Espeon.Core.Databases.CommandStore;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface ICommandHandlingService {
		Task SetupCommandsAsync(CommandStore commandStore);

		Task ExecuteCommandAsync(SocketUser author, ISocketMessageChannel channel, string content,
			SocketUserMessage message);
	}
}