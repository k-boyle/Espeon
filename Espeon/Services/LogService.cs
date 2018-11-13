using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Core.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pusharp;
using Pusharp.Entities;

namespace Espeon.Services
{
    [Service(typeof(ILogService), true)]
    public class LogService : ILogService
    {
        [Inject] private readonly PushBulletClient _push;

        private Device _phone;
        private Device Phone => _phone ?? (_phone = _push.Devices.First());

        private readonly SemaphoreSlim _semaphore;

        public LogService()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task LogAsync(Source source, Severity severity, string message, Exception ex = null)
        {
            await _semaphore.WaitAsync();

            var time = DateTime.UtcNow;
            Console.Write($"{(time.Hour < 10 ? "0" : "")}{time.Hour}:{(time.Minute < 10 ? "0" : "")}{time.Minute}:{(time.Second < 10 ? "0" : "")}{time.Second} ");
            Console.Write("[");

            switch (severity)
            {
                case Severity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    await Phone.SendNoteAsync(x =>
                    {
                        x.Title = "Critical Error";
                        x.Body = $"{message}\n{ex}";
                    });
                    break;
                case Severity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Severity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case Severity.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case Severity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Severity.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.Write($"{severity,-8}");
            Console.ResetColor();
            Console.Write("]");

            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"{source,-9}");
            Console.ResetColor();
            Console.Write("] ");

            if (!string.IsNullOrEmpty(message))
                Console.Write(string.Join("", message.Where(x => !char.IsControl(x))));

            Console.Write(ex?.ToString());

            Console.WriteLine();
            _semaphore.Release();
        }
    }
}
