using Discord;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Core;
using ConsoleColour = System.ConsoleColor;

namespace Espeon.Services
{
    [Service]
    public class LogService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

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
                    Console.ForegroundColor = ConsoleColour.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColour.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColour.DarkYellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColour.DarkGreen;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColour.Green;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColour.Magenta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Console.Write($"{severity, -8}");
            Console.ResetColor();
            Console.Write("]");

            Console.Write("[");
            Console.ForegroundColor = ConsoleColour.Cyan;
            Console.Write($"{source, -11}");
            Console.ResetColor();
            Console.Write("] ");

            if(!string.IsNullOrEmpty(message))
                Console.Write(string.Join("", message.Where(x => !char.IsControl(x))));

            if (!string.IsNullOrEmpty(exception?.ToString()))
            {
                Console.Write(log.Exception?.ToString());
                Console.Write(exception);
            }

            Console.WriteLine();
            _semaphore.Release();
        }

        public void NewLogEvent(LogSeverity serverity, LogSource source, string message)
            => _ = LogEvent(new LogMessage(serverity, source.ToString(), message));
    }
}
