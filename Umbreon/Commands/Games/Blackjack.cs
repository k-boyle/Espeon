using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Extensions;
using Umbreon.Helpers;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Paginator;
using Umbreon.Interfaces;
using Umbreon.Services;
using Colour = Discord.Color;

namespace Umbreon.Commands.Games
{
    public class Blackjack : IGame, IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public ICommandContext Context { get; }
        private IUserMessage Message { get; set; }
        private InteractiveService Interactive { get; }
        private GamesService Games { get; }
        private CandyService Candy { get; }

        private readonly IReadOnlyDictionary<string, int> _cards = new Dictionary<string, int>
        {
            { "ace", 11 },
            { "two", 2 },
            { "three", 3 },
            { "four", 4 },
            { "five", 5 },
            { "six", 6 },
            { "seven", 7 },
            { "eight", 8 },
            { "nine", 9 },
            { "ten", 10 },
            { "jack", 10 },
            { "queen", 10 },
            { "king", 10 }
        };

        private readonly int _bet;
        private bool _inGame = true;

        private readonly IReadOnlyCollection<string> _suits = new[] { "❤", "♦", "♣", "♠" };

        private readonly Queue<(string suit, string card, int value)> _deck;

        private readonly List<(string suit, string card, int value)> _playerCards = new List<(string suit, string card, int value)>();
        private readonly List<(string suit, string card, int value)> _dealerCards = new List<(string suit, string card, int value)>();
        
        private readonly Emoji _hit = new Emoji("➕");
        private readonly Emoji _stop = new Emoji("❌");

        private readonly MessageService _message;

        public Blackjack(ICommandContext context, int bet, IServiceProvider services)
        {
            Context = context;
            _bet = bet;
            _message = services.GetService<MessageService>();
            Interactive = services.GetService<InteractiveService>();
            Games = services.GetService<GamesService>();
            Candy = services.GetService<CandyService>();
            _deck = new Queue<(string, string, int)>((from suit in _suits from card in _cards select (suit, card.Key, card.Value)).OrderBy(_ => services.GetService<Random>().Next()));
        }

        public async Task StartAsync()
        {
            var message = await _message.SendMessageAsync(Context, "Starting blackjack...");
            Message = message;

            _playerCards.Add(DrawCard());
            _playerCards.Add(DrawCard());

            if (_playerCards.Sum(x => x.value) == 21)
            {
                await EndAsync();
                return;
            }

            _dealerCards.Add(DrawCard());

            await message.ModifyAsync(x =>
            {
                x.Content = string.Empty;
                x.Embed = BuildEmbed();
            });

            _ = message.AddReactionsAsync(new RequestOptions
            {
                BypassBuckets = true
            },
                _hit,
                _stop);
            Interactive.AddReactionCallback(Message, this);
            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(async _ =>
            {
                Interactive.RemoveReactionCallback(Message);
                if (_inGame)
                {
                    await Message.ModifyAsync(x =>
                    {
                        x.Content = string.Empty;
                        x.Embed = TimeoutEmbed();
                    });
                    Candy.SetCandies(Context.User.Id, Candy.GetCandies(Context.User.Id) - _bet);
                    _inGame = false;
                }
                Games.LeaveGame(Context.User.Id);
                _ = Message.RemoveAllReactionsAsync();
            });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(_hit))
            {
                var (suit, card, value) = DrawCard();
                _playerCards.Add((suit, card, value));
                var playerTotal = _playerCards.Sum(x => x.value);
                if (playerTotal > 21)
                {
                    if(_playerCards.All(x => x.card != "ace"))
                    {
                        await EndAsync();
                        return true;
                    }

                    while (playerTotal > 21)
                    {
                        var first = _playerCards.FirstOrDefault(x => x.card == "ace" && x.value != 1);
                        if (first.suit is null && first.card is null && first.value is 0)
                        {
                            await EndAsync();
                            return true;
                        }

                        _playerCards[_playerCards.IndexOf(first)] = (first.suit, first.card, 1);
                        playerTotal = _playerCards.Sum(x => x.value);
                    }
                }

                if (playerTotal == 21)
                {
                    await EndAsync();
                    return true;
                }
            }

            if (emote.Equals(_stop))
            {
                await EndAsync();
                return true;
            }

            _ = Message.RemoveReactionAsync(emote, reaction.User.Value);

            await Message.ModifyAsync(x => x.Embed = BuildEmbed());
            return false;
        }

        private Embed BuildEmbed()
        {
            var builder = new EmbedBuilder
            {
                Title = "Blackjack",
                Description = $"You have bet {_bet}{EmotesHelper.Emotes["rarecandy"]} candies\n" +
                              $"A game of blackjack. Click {_hit} to hit or {_stop} to stay\n",
                Color = Colour.Default
            };
            builder.AddField("Player", $"Your cards are: {string.Join(", ", _playerCards.Select(x => $"[{x.card} of {x.suit}]"))}\n" +
                                       $"For a total of: {_playerCards.Sum(x => x.value)}");
            builder.AddField("Umbreon", $"My cards are {string.Join(", ", _dealerCards.Select(x => $"[{x.card} of {x.suit}]"))}\n" +
                                       $"For a total of: {_dealerCards.Sum(x => x.value)}");

            return builder.Build();
        }

        private (string suit, string card, int value) DrawCard()
            => _deck.Dequeue();

        public async Task EndAsync()
        {
            _inGame = false;
            Games.LeaveGame(Context.User.Id);
            _ = Message.RemoveAllReactionsAsync();
            await Message.ModifyAsync(x =>
            {
                x.Content = string.Empty;
                x.Embed = BuildEmbed();
            });

            var playerTotal = _playerCards.Sum(x => x.value);
            if (playerTotal > 21)
            {
                await Message.ModifyAsync(x => x.Embed = LoseEmbed());
                Candy.SetCandies(Context.User.Id, Candy.GetCandies(Context.User.Id) - _bet);
                Games.LeaveGame(Context.User.Id);
                _inGame = false;
                return;
            }

            var dealerTotal = _dealerCards.Sum(x => x.value);
            while (dealerTotal < 17)
            {
                var (suit, card, value) = DrawCard();
                _dealerCards.Add((suit, card, value));
                dealerTotal = _dealerCards.Sum(x => x.value);

                if (dealerTotal <= 21) continue;
                {
                    if (_dealerCards.All(x => x.card != "ace"))
                        break;

                    while (dealerTotal > 21 && _dealerCards.Any(x => x.card == "ace" && x.value == 11))
                    {
                        var first = _dealerCards.First(x => x.card == "ace");
                        _dealerCards[_dealerCards.IndexOf(first)] = (first.suit, first.card, 1);
                        dealerTotal = _dealerCards.Sum(x => x.value);
                    }
                }
            }

            if (dealerTotal > 21)
            {
                await Message.ModifyAsync(x => x.Embed = WinEmbed());
                Candy.SetCandies(Context.User.Id, Candy.GetCandies(Context.User.Id) + (int)(0.5 * _bet));
            }
            else if (dealerTotal > playerTotal)
            {
                await Message.ModifyAsync(x => x.Embed = LoseEmbed());
                Candy.SetCandies(Context.User.Id, Candy.GetCandies(Context.User.Id) - _bet);
            }
            else if (dealerTotal < playerTotal)
            {
                await Message.ModifyAsync(x => x.Embed = WinEmbed());
                Candy.SetCandies(Context.User.Id, Candy.GetCandies(Context.User.Id) + (int)(0.5 * _bet));
            }
            else if (dealerTotal == playerTotal)
                await Message.ModifyAsync(x => x.Embed = DrawEmbed());
        }

        private Embed LoseEmbed()
            => new EmbedBuilder
                {
                    Title = "Blackjack Result",
                    Description = $"I win! You lose {_bet}{EmotesHelper.Emotes["rarecandy"]} candies!",
                    Color = Colour.Red
                }
                .AddField("Player",
                    $"Your cards are: {string.Join(", ", _playerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_playerCards.Sum(x => x.value)}")
                .AddField("Umbreon",
                    $"My cards are: {string.Join(", ", _dealerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_dealerCards.Sum(x => x.value)}")
                .Build();

        private Embed WinEmbed()
            => new EmbedBuilder
                {
                    Title = "Blackjack Result",
                    Description = $"You win! You win {(int)(0.5 * _bet)}{EmotesHelper.Emotes["rarecandy"]} candies!",
                    Color = Colour.Green
                }
                .AddField("Player",
                    $"Your cards are: {string.Join(", ", _playerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_playerCards.Sum(x => x.value)}")
                .AddField("Umbreon",
                    $"My cards are: {string.Join(", ", _dealerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_dealerCards.Sum(x => x.value)}")
                .Build();

        private Embed DrawEmbed()
            => new EmbedBuilder
                {
                    Title = "Blackjack Result",
                    Description = "We draw! You don't win/lose any candies!",
                    Color = Colour.Orange
                }
                .AddField("Player",
                    $"Your cards are: {string.Join(", ", _playerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_playerCards.Sum(x => x.value)}")
                .AddField("Umbreon",
                    $"My cards are: {string.Join(", ", _dealerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_dealerCards.Sum(x => x.value)}")
                .Build();

        private Embed TimeoutEmbed()
            => new EmbedBuilder
                {
                    Title = "Blackjack Result",
                    Description = $"You took too long to respond, so I win! You lose {_bet}{EmotesHelper.Emotes["rarecandy"]} candies!",
                    Color = Colour.Red
                }
                .AddField("Player",
                    $"Your cards are: {string.Join(", ", _playerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_playerCards.Sum(x => x.value)}")
                .AddField("Umbreon",
                    $"My cards are: {string.Join(", ", _dealerCards.Select(y => $"[{y.card} of {y.suit}]"))}\n" +
                    $"For a total of: {_dealerCards.Sum(x => x.value)}")
                .Build();
    }
}
