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

		public StatusService(IServiceProvider services) : base(services) { }

		async Task IStatusService.RunStatusesAsync() {
			int last = -1;

			while (true) {
				(ActivityType, string)[] statuses = {
					(ActivityType.Watching, "you"),
					(ActivityType.Playing, $"with {this._client.Guilds.Sum(x => x.Value.MemberCount)} people"),
					(ActivityType.Listening, "Pokemon opening theme"),
					(ActivityType.Watching, $"over {this._commands.GetAllCommands().Count} commands")
				};

				int next;

				do {
					next = this._random.Next(statuses.Length);
				} while (next == last);

				(ActivityType activityType, string str) = statuses[next];

				await this._client.SetPresenceAsync(new LocalActivity(str, activityType));

				last = next;
				await Task.Delay(Delay);
			}
		}
	}
}