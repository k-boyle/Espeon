using Espeon.Core.Commands;
using Espeon.Core.Entities;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface ICustomCommandsService
    {
        Task<bool> TryCreateCommandAsync(IEspeonContext context, string name, string value);
        Task<bool> TryDeleteCommandAsync(IEspeonContext context, BaseCustomCommand command);
        Task<bool> TryModifyCommandAsync(IEspeonContext context, BaseCustomCommand command, string newValue);
        Task<ImmutableArray<BaseCustomCommand>> GetCommandsAsync(ulong id);
    }
}
