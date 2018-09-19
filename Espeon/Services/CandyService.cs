using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Espeon.Attributes;
using Espeon.Core.Entities.User;
using Espeon.Helpers;

namespace Espeon.Services
{
    [Service]
    public class CandyService
    {
        private readonly DatabaseService _database;
        private readonly Random _random;
        private readonly DiscordSocketClient _client;

        private const float WinConstant = 0.2f;
        private const int LogAmount = 5;

        private readonly ConcurrentQueue<(ulong Id, int Amount)> _transactionLog = new ConcurrentQueue<(ulong, int)>();

        public CandyService(DatabaseService database, Random random, DiscordSocketClient client)
        {
            _database = database;
            _random = random;
            _client = client;
        }

        public async Task UpdateCandiesAsync(ulong id, bool isClaim, int amount, bool fromMessage = false, bool isGift = false)
        {
            var bot = await _database.GetBotUserAsync();
            if (id == bot.Id) return;
            if (amount < 0 && !isGift)
            {
                bot.RareCandies -= amount;
                _database.UpdateObject("users", bot);
            }

            if(!isClaim && !fromMessage)
                Log(id, amount);

            var user = await _database.GetObjectAsync<UserObject>("users", id);
            user.RareCandies += amount;
            if (isClaim)
                user.LastClaimed = DateTime.UtcNow;
            _database.UpdateObject("users", user);
        }

        public async Task<bool> TryStealAsync(ulong id, int amount)
        {
            var bot = await _database.GetBotUserAsync();
            var botCandies = bot.RareCandies;
            var chance = (float)amount / botCandies * WinConstant;

            if (chance < _random.NextDouble())
            {
                await UpdateCandiesAsync(id, false, -amount);
                return false;
            }

            await UpdateCandiesAsync(id, false, bot.RareCandies);
            Log(id, bot.RareCandies);
            bot.RareCandies = 0;
            _database.UpdateObject("users", bot);
            return true;
        }

        private void Log(ulong id, int amount)
        {
            if (amount == 0) return;

            if (_transactionLog.Count == LogAmount)
                _transactionLog.TryDequeue(out _);

            _transactionLog.Enqueue((id, amount));
        }

        public string GetLogs()
            => string.Join('\n', _transactionLog.Select(x => $"{_client.GetUser(x.Id)?.Username ?? $"{x.Id}"} {(x.Amount < 0 ? "lost" : "won")} " +
                                                             $"{(x.Amount < 0 ? -x.Amount : x.Amount)}{EmotesHelper.Emotes["rarecandy"]} rare candies!"));

        public async Task<bool> CanClaimAsync(ulong id)
        {
            var user = await _database.GetObjectAsync<UserObject>("users", id);
            return DateTime.UtcNow - user.LastClaimed > TimeSpan.FromHours(8);
        }

        public async Task<int> GetCandiesAsync(ulong id)
            => (await _database.GetObjectAsync<UserObject>("users", id)).RareCandies;
    }
}
