using Espeon.Commands;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface IGamesService
    {
        Task<bool> TryStartGameAsync(EspeonContext context, IGame game, TimeSpan timeout);
        Task<bool> TryLeaveGameAsync(EspeonContext context);
    }
}
