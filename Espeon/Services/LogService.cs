using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Core.Services;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(typeof(ILogService), true)]
    public class LogService : ILogService
    {
        public Task LogAsync(Source source, Severity severity, string message, Exception ex = null)
        {
            throw new NotImplementedException();
        }
    }
}
