using Casino.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class LogService : BaseService<InitialiseArgs>, ILogService {
		[Inject] private readonly DiscordSocketClient _client;

		private readonly object _lock;

		private const ulong LogChannelId = 574891410495373323;
		private IMessageChannel LogChannel => this._client.GetChannel(LogChannelId) as IMessageChannel;

		public LogService(IServiceProvider services) : base(services) {
			this._lock = new object();

			this._client.JoinedGuild += guild => BotLogAsync($"Joined: {guild.Name} with {guild.MemberCount} members");

			this._client.LeftGuild += guild => BotLogAsync($"Left: {guild.Name}");
		}

		void ILogService.Log(Source source, Severity severity, string message, Exception ex) {
			lock (this._lock) {
				DateTimeOffset time = DateTimeOffset.UtcNow;
				Console.Write($"{FormatTime(time)} ");
				Console.Write("[");

				Console.ForegroundColor = severity switch {
					Severity.Critical => ConsoleColor.DarkRed,
					Severity.Error    => ConsoleColor.Red,
					Severity.Warning  => ConsoleColor.DarkYellow,
					Severity.Info     => ConsoleColor.Cyan,
					Severity.Verbose  => ConsoleColor.Green,
					Severity.Debug    => ConsoleColor.Magenta,
					_                 => throw new ArgumentOutOfRangeException()
				};

				Console.Write($"{severity,-8}");
				Console.ResetColor();
				Console.Write("]");

				Console.Write("[");
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.Write($"{source,-9}");
				Console.ResetColor();
				Console.Write("] ");

				if (!string.IsNullOrEmpty(message)) {
					Console.Write(string.Join("", message.Where(x => !char.IsControl(x))));
				}

				Console.Write(ex?.ToString());

				Console.WriteLine();
			}
		}

		Task ILogService.BotLogAsync(string message) {
			return BotLogAsync(message);
		}

		private Task BotLogAsync(string message) {
			return LogChannel.SendMessageAsync($"[{FormatTime(DateTimeOffset.UtcNow)}] {message}");
		}

		private static string FormatTime(DateTimeOffset time) {
			return $"{(time.Hour < 10 ? "0" : "")}{time.Hour}:{(time.Minute < 10 ? "0" : "")}" +
			       $"{time.Minute}:{(time.Second < 10 ? "0" : "")}{time.Second}";
		}
	}
}