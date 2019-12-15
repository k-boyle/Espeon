using Disqord;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class StatusService : BaseService<InitialiseArgs>, IStatusService {
		[Inject] private readonly CommandService _commands;
		[Inject] private readonly DiscordClient _client;
		[Inject] private readonly Random _random;

		private static readonly TimeSpan Delay = TimeSpan.FromMinutes(1);

		private readonly Func<(ActivityType, string)>[] _statuses;

		public StatusService(IServiceProvider services) : base(services) {
			this._statuses = new Func<(ActivityType, string)>[] {
				() => (ActivityType.Watching, "you"),
				() => (ActivityType.Playing, $"with {this._client.Guilds.Sum(x => x.Value.MemberCount)} people"),
				() => (ActivityType.Listening, "Pokemon opening theme"),
				() => (ActivityType.Watching, $"over {this._commands.GetAllCommands().Count} commands")
			};
		}

		async Task IStatusService.RunStatusesAsync() {
			while (true) {
				int next = this._random.Next(this._statuses.Length);
				(ActivityType activityType, string str) = this._statuses[next]();
				
				await this._client.SetPresenceAsync(new LocalActivity(str, activityType));

				await Task.Delay(Delay);
			}
		}
	}
}