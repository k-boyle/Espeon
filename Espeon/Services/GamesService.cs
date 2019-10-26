using Casino.DependencyInjection;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class GamesService : BaseService<InitialiseArgs>, IGamesService {
		[Inject] private readonly IServiceProvider _services;
		[Inject] private readonly IInteractiveService _interactive;

		private readonly ConcurrentDictionary<ulong, IGame> _games;

		public GamesService(IServiceProvider services) : base(services) {
			this._games = new ConcurrentDictionary<ulong, IGame>();
		}

		async Task<bool> IGamesService.TryStartGameAsync(EspeonContext context, IGame game, TimeSpan timeout) {
			if (this._games.ContainsKey(context.User.Id)) {
				return false;
			}

			this._services.Inject(game);

			bool res = await game.StartAsync();

			if (!res) {
				this._games[context.User.Id] = game;
			}

			return res || await this._interactive.TryAddCallbackAsync(game, timeout);
		}

		async Task<bool> IGamesService.TryLeaveGameAsync(EspeonContext context) {
			if (!this._games.TryGetValue(context.User.Id, out IGame game)) {
				return false;
			}

			if (!this._interactive.TryRemoveCallback(game)) {
				return false;
			}

			await game.EndAsync();

			return this._games.TryRemove(context.User.Id, out _);

		}
	}
}