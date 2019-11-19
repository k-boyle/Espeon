using Espeon.Commands;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class GamesService : BaseService<InitialiseArgs>, IGamesService<IGame> {
		[Inject] private readonly IServiceProvider _services;
		[Inject] private readonly IInteractiveService<IReactionCallback, EspeonContext> _interactive;

		private readonly ConcurrentDictionary<ulong, IGame> _games;

		public GamesService(IServiceProvider services) : base(services) {
			this._games = new ConcurrentDictionary<ulong, IGame>();
		}

		async Task<bool> IGamesService<IGame>.TryStartGameAsync(ulong userId, IGame game, TimeSpan timeout) {
			if (this._games.ContainsKey(userId)) {
				return false;
			}

			this._services.Inject(game);

			bool res = await game.StartAsync();

			if (!res) {
				this._games[userId] = game;
			}

			return res || await this._interactive.TryAddCallbackAsync(game, timeout);
		}

		async Task<bool> IGamesService<IGame>.TryLeaveGameAsync(ulong userId) {
			if (!this._games.TryGetValue(userId, out IGame game)) {
				return false;
			}

			if (!this._interactive.TryRemoveCallback(game)) {
				return false;
			}

			await game.EndAsync();

			return this._games.TryRemove(userId, out _);

		}
	}
}