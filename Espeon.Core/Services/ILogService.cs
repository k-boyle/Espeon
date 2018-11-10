using System.Threading.Tasks;
using Discord;

namespace Espeon.Core.Services
{
    public interface ILogService
    {
        Task LogAsync(LogMessage message);
    }
}
