using Casino.DependencyInjection;
using Espeon.Commands;
using Espeon.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class GamesService : BaseService<InitialiseArgs>, IGamesService
    {
        [Inject] private readonly IServiceProvider _services;
        [Inject] private readonly IInteractiveService _interactive;

        private readonly ConcurrentDictionary<ulong, IGame> _games;

        public GamesService(IServiceProvider services) : base(services)
        {
            _games = new ConcurrentDictionary<ulong, IGame>();
        }

        async Task<bool> IGamesService.TryStartGameAsync(EspeonContext context, IGame game, TimeSpan timeout)
        {
            if (_games.ContainsKey(context.User.Id))
                return false;

            _services.Inject(game);

            var res = await game.StartAsync();

            if (!res)
                _games[context.User.Id] = game;

            return res || await _interactive.TryAddCallbackAsync(game, timeout);
        }

        async Task<bool> IGamesService.TryLeaveGameAsync(EspeonContext context)
        {
            if (!_games.TryGetValue(context.User.Id, out var game))
                return false;

            if (!_interactive.TryRemoveCallback(game))
                return false;

            await game.EndAsync();

            return _games.TryRemove(context.User.Id, out _);

        }
    }
}
