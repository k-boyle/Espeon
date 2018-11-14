using Espeon.Core.Commands;
using Espeon.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface ICustomCommandsService
    {
        Task<bool> TryCreateCommandAsync(IEspeonContext context, string name, string value);
        Task TryDeleteCommandAsync(IEspeonContext context, BaseCustomCommand command);
        Task TryModifyCommandAsync(IEspeonContext context, BaseCustomCommand command, string newValue);
        Task<IReadOnlyCollection<BaseCustomCommand>> GetCommandsAsync(ulong id);
    }
}
