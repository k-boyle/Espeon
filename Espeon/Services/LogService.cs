using Casino.Common.DependencyInjection;
using System;
using System.Linq;

namespace Espeon.Services
{
    public class LogService : BaseService<InitialiseArgs>
    { 
        private readonly object _lock;

        public LogService(IServiceProvider services) : base(services)
        {
            _lock = new object();
        }

        public void Log(Source source, Severity severity, string message, Exception ex = null)
        {
            lock (_lock)
            {
                var time = DateTime.UtcNow;
                Console.Write($"{(time.Hour < 10 ? "0" : "")}{time.Hour}:{(time.Minute < 10 ? "0" : "")}" +
                              $"{time.Minute}:{(time.Second < 10 ? "0" : "")}{time.Second} ");
                Console.Write("[");

                switch (severity)
                {
                    case Severity.Critical:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                    case Severity.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case Severity.Warning:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case Severity.Info:
                        Console.ForegroundColor = ConsoleColor.Cyan;
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
            }
        }
    }
}
