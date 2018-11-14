using System.Collections.Generic;
using System.Threading.Tasks;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Entities;
using Espeon.Core.Services;

namespace Espeon.Services
{
    [Service(typeof(ICustomCommandsService), true)]
    public class CustomCommandsService : ICustomCommandsService
    {
        public Task<bool> TryCreateCommandAsync(IEspeonContext context, string name, string value)
        {
            throw new System.NotImplementedException();
        }

        public Task TryDeleteCommandAsync(IEspeonContext context, BaseCustomCommand command)
        {
            throw new System.NotImplementedException();
        }

        public Task TryModifyCommandAsync(IEspeonContext context, BaseCustomCommand command, string newValue)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<BaseCustomCommand>> GetCommandsAsync(ulong id)
        {
            throw new System.NotImplementedException();
        }
    }
}
