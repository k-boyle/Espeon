using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;

namespace Umbreon.Services
{
    [Service]
    public class LogService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public LogService(DiscordSocketClient client, CommandService commands)
        {
            _client = client;
            _commands = commands;
        }

        public async Task LogEvent(LogMessage log)
        {
            await _semaphore.WaitAsync();

            var source = log.Source;
            var message = log.Message;
            var exception = log.Exception?.InnerException;
            var severity = log.Severity;

            var time = DateTime.UtcNow;

            Console.Write($"{(time.Hour < 10 ? "0" : "")}{time.Hour}:{(time.Minute < 10 ? "0" : "")}{time.Minute}:{(time.Second < 10 ? "0" : "")}{time.Second} ");

            Console.Write("[");
            switch (severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            const int sevLength = 8;
            if (severity.ToString().Length < sevLength)
            {
                var builder = new StringBuilder(sevLength);
                builder.Append(severity.ToString());
                builder.Append(' ', sevLength - severity.ToString().Length);
                Console.Write($"{builder}");
            }
            else if (severity.ToString().Length > sevLength)
            {
                Console.Write($"{severity.ToString().Substring(0, sevLength)}");
            }
            else
            {
                Console.Write(severity.ToString());
            }
            Console.ResetColor();
            Console.Write("]");

            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            if (source.Length < 11)
            {
                var builder = new StringBuilder(11);
                builder.Append(source);
                builder.Append(' ', 11 - source.Length);
                Console.Write($"{builder}");
            }
            else if (source.Length > 11)
            {
                Console.Write($"{source.Substring(0, 11)}");
            }
            else
            {
                Console.Write(source);
            }
            Console.ResetColor();
            Console.Write("] ");

            if(!string.IsNullOrEmpty(message))
                Console.Write(string.Join("", message.Where(x => !char.IsControl(x))));

            if (!string.IsNullOrEmpty(exception?.ToString()))
            {
                Console.Write(log.Exception.ToString());
                Console.Write(exception);
            }

            Console.WriteLine();
            _semaphore.Release();
        }

        public void NewLogEvent(LogSeverity serverity, LogSource source, string message)
        {
            _ = LogEvent(new LogMessage(serverity, source.ToString(), message));
        }
    }
}
