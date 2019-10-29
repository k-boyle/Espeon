using Disqord;
using Espeon.Core.Database.CommandStore;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface ICommandHandlingService {
		Task SetupCommandsAsync(CommandStore commandStore);

		Task ExecuteCommandAsync(CachedUser author, ITextChannel channel, string content,
			CachedUserMessage message);
	}
}