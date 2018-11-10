using System;
using System.Threading.Tasks;
using Discord;
using Espeon.Core.Attributes;
using Espeon.Core.Services;

namespace Espeon.Services
{
    [Service(typeof(ILogService), true)]
    public class LogService : ILogService
    {
        public Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}
