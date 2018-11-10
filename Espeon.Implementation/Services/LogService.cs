using Discord;
using Espeon.Core.Attributes;
using Espeon.Core.Services;
using System;
using System.Threading.Tasks;

namespace Espeon.Implementation.Services
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
