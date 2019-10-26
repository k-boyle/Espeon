using Espeon.Core.Commands;
using Espeon.Core.Databases;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface ICustomCommandsService {
		Task<bool> TryCreateCommandAsync(EspeonContext context, string name, string value);
		Task DeleteCommandAsync(EspeonContext context, CustomCommand command);
		Task ModifyCommandAsync(EspeonContext context, CustomCommand command, string newValue);
		Task<ImmutableArray<CustomCommand>> GetCommandsAsync(EspeonContext context);
		bool IsCustomCommand(ulong id);
	}
}