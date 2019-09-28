using Casino.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Espeon.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class LogService : BaseService<InitialiseArgs>, ILogService
    {
        [Inject] private readonly DiscordSocketClient _client;

        private readonly object _lock;

        private const ulong LogChannelId = 574891410495373323;
        private IMessageChannel LogChannel => _client.GetChannel(LogChannelId) as IMessageChannel;

        public LogService(IServiceProvider services) : base(services)
        {
            _lock = new object();

            _client.JoinedGuild += guild
                => BotLogAsync($"Joined: {guild.Name} with {guild.MemberCount} members");

            _client.LeftGuild += guild
                => BotLogAsync($"Left: {guild.Name}");
        }

        void ILogService.Log(Source source, Severity severity, string message, Exception ex = null)
        {
            lock (_lock)
            {
                var time = DateTimeOffset.UtcNow;
                Console.Write($"{FormatTime(time)} ");
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

        Task ILogService.BotLogAsync(string message)
            => BotLogAsync(message);

        private Task BotLogAsync(string message)
            => LogChannel.SendMessageAsync($"[{FormatTime(DateTimeOffset.UtcNow)}] {message}");

        private static string FormatTime(DateTimeOffset time)
            => $"{(time.Hour < 10 ? "0" : "")}{time.Hour}:{(time.Minute < 10 ? "0" : "")}" +
               $"{time.Minute}:{(time.Second < 10 ? "0" : "")}{time.Second}";
    }
}
