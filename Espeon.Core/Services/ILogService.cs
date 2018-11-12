using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface ILogService
    {
        Task LogAsync(Source source, Severity severity, string message, Exception ex = null);
    }
}
