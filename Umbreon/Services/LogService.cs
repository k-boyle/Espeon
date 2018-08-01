using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Discord.Addons.Interactive.Interfaces;
using Umbreon.Core;

namespace Umbreon.Services
{
    public class LogService : IService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public LogService(DiscordSocketClient client, CommandService commands)
        {
            _client = client;
            _commands = commands;
        }

        public Task LogEvent(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public void NewLogEvent(LogSeverity serverity, LogSource source, string message)
        {
            LogEvent(new LogMessage(serverity, source.ToString(), message));
        }
    }
}
