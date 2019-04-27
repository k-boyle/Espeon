using Casino.Common.DependencyInjection;
using Espeon.Commands;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class GamesService : BaseService<InitialiseArgs>
    {
        [Inject] private readonly IServiceProvider _services;
        [Inject] private readonly InteractiveService _interactive;

        private readonly ConcurrentDictionary<ulong, IGame> _games;

        public GamesService(IServiceProvider services) : base(services)
        {
            _games = new ConcurrentDictionary<ulong, IGame>();
        }

        public async Task<bool> TryStartGameAsync(EspeonContext context, IGame game, TimeSpan timeout)
        {
            if (_games.ContainsKey(context.User.Id))
                return false;

            _services.Inject(game);

            var res = await game.StartAsync();

            if (!res)
                _games[context.User.Id] = game;

            return res || await _interactive.TryAddCallbackAsync(game, timeout);
        }

        public async Task<bool> TryLeaveGameAsync(EspeonContext context)
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
