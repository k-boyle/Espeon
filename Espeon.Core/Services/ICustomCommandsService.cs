using Disqord;
using Espeon.Core.Database;
using Espeon.Core.Database.GuildStore;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface ICustomCommandsService {
		Task<bool> TryCreateCommandAsync(GuildStore guildStore, IGuild guild, string name, string value);
		Task DeleteCommandAsync(GuildStore guildStore, IGuild guild, CustomCommand command);
		Task ModifyCommandAsync(GuildStore guildStore, CustomCommand command, string newValue);
		Task<ImmutableArray<CustomCommand>> GetCommandsAsync(GuildStore guildStore, IGuild guild);
		bool IsCustomCommand(ulong id);
	}
}