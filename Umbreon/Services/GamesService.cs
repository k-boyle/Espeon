using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Commands;
using Umbreon.Attributes;
using Umbreon.Interfaces;

namespace Umbreon.Services
{
    [Service]
    public class GamesService
    {
        private readonly ConcurrentDictionary<ulong, IGame> _currentGames = new ConcurrentDictionary<ulong, IGame>();

        public async Task StartGameAsync(ulong userId, IGame game)
        {
            if (_currentGames.TryAdd(userId, game))
                await game.StartAsync();
        }

        public void LeaveGame(ulong userId)
            => _currentGames.TryRemove(userId, out _);

        public bool InGame(ICommandContext context)
            => _currentGames.ContainsKey(context.User.Id);
    }
}
