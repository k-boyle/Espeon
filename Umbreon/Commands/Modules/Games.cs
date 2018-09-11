using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.Games;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.Preconditions;
using Umbreon.Commands.TypeReaders;
using Umbreon.Core;
using Umbreon.Services;
using Remarks = Umbreon.Attributes.RemarksAttribute;

namespace Umbreon.Commands.Modules
{
    [Name("Games")]
    [Summary("Some small games to play in the guild")]
    public class Games : UmbreonBase
    {
        private readonly GamesService _games;
        private readonly CandyService _candy;
        private readonly Random _random;

        public Games(GamesService games, CandyService candy, Random random)
        {
            _games = games;
            _candy = candy;
            _random = random;
        }

        [Command("blackjack")]
        [Alias("bj")]
        [Summary("Start a game of blackjack")]
        [Name("Blackjack")]
        [Usage("blackjack 10")]
        [RequireGame(false)]
        public Task BlackJack(
            [Name("Amount To Bet")]
            [Summary("The amount of rare candies you want to bet.")]
            [Remarks("Don't specify for no bet")]
            [OverrideTypeReader(typeof(CandyTypeReader))] int amount = 0)
            => _games.StartGameAsync(Context.User.Id, new Blackjack(Context, amount, Services));

        [Command("coinflip")]
        [Alias("cf")]
        [Summary("Flip a coin")]
        [Name("Coin Flip")]
        [Usage("coinflip heads 100")]
        [RequireGame(false)]
        public async Task CoinFlip(
            [Name("Face")]
            [Summary("Heads or tails")] Face choice,
            [Name("Amount")]
            [Summary("The amount of candies you want to bet")]
            [OverrideTypeReader(typeof(CandyTypeReader))] int amount = 0)
        {
            var flip = _random.Next(100) > 50 ? Face.Heads : Face.Tails;

            _candy.UpdateCandies(Context.User.Id, false, flip == choice ? (int)(0.5 * amount) : -(int)amount);
            await SendMessageAsync($"It was {flip}! {(flip == choice ? "You win!" : "You lose!")}");
        }

        [Command("duel")]
        [Summary("Challenge someone to a duel")]
        [Name("Duel")]
        [Usage("Duel Umbreon 100")]
        [RequireGame(false)]
        public Task Duel(
            [Name("User")]
            [Summary("The user you want to duel")] SocketGuildUser user,
            [Name("Amount")]
            [Summary("The amount you want to wager")]
            [OverrideTypeReader(typeof(CandyTypeReader))] int amount = 0)
            => _games.StartGameAsync(Context.User.Id, new Duel(Context, user, (int)amount, Services));
    }
}
