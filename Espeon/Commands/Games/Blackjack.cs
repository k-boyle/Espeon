using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Espeon.Commands.Contexts;
using Espeon.Extensions;
using Espeon.Helpers;
using Espeon.Interactive;
using Espeon.Interactive.Callbacks;
using Espeon.Interactive.Criteria;
using Espeon.Interactive.Paginator;
using Espeon.Interfaces;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Colour = Discord.Color;

namespace Espeon.Commands.Games
{
    public class Blackjack : IReactionCallback, IGame
    {
        public RunMode RunMode => RunMode.Async;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context => context;

        //not following naming convention because I *hate* referencing my context using _context
        private readonly EspeonContext context;
        private readonly GamesService _games;
        private readonly CandyService _candy;
        private readonly InteractiveService _interactive;
        private readonly MessageService _messageService;
        private readonly SemaphoreSlim _semaphore;

        private IUserMessage _message;

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

        private readonly IReadOnlyCollection<string> _suits = new[] { "❤", "♦", "♣", "♠" };
        private readonly Queue<(string suit, string card, int value)> _deck;
        private List<(string suit, string card, int value)> _playerCards;
        private List<(string suit, string card, int value)> _dealerCards;

        private readonly Emoji _hit = new Emoji("➕");
        private readonly Emoji _stop = new Emoji("❌");

        private readonly int _bet;

        private bool _finished = false;

        public Blackjack(EspeonContext context, IServiceProvider services, int bet)
        {
            this.context = context;
            _games = services.GetService<GamesService>();
            _candy = services.GetService<CandyService>();
            _interactive = services.GetService<InteractiveService>();
            _messageService = services.GetService<MessageService>();
            _semaphore = new SemaphoreSlim(1, 1);

            _bet = bet;

            _deck = new Queue<(string, string, int)>((from suit in _suits from card in _cards select (suit, card.Key, card.Value))
                    .OrderBy(_ => services.GetService<Random>().Next()));
            _playerCards = new List<(string suit, string card, int value)>();
            _dealerCards = new List<(string suit, string card, int value)>();
        }

        public async Task StartAsync()
        {
            _playerCards.Add(_deck.Dequeue());
            _dealerCards.Add(_deck.Dequeue());
            _playerCards.Add(_deck.Dequeue());

            _message = await _messageService.SendMessageAsync(context, string.Empty, embed: BuildEmbed());

            var playerTotal = CalculateTotal(ref _playerCards);

            if (playerTotal == 21)
            {
                await EndAsync();
                return;
            }

            _ = Task.Run(async () =>
            {
                await _message.AddReactionAsync(_hit, new RequestOptions
                {
                    BypassBuckets = true
                });

                await _message.AddReactionAsync(_stop, new RequestOptions
                {
                    BypassBuckets = true
                });
            });

            _interactive.AddReactionCallback(_message, this);

            _ = Task.Delay(Timeout.GetValueOrDefault())
                .ContinueWith(async _ =>
                {
                    await EndAsync();
                });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            await _semaphore.WaitAsync();

            var emote = reaction.Emote;

            if (emote.Equals(_hit))
            {
                _playerCards.Add(_deck.Dequeue());
                var playerTotal = CalculateTotal(ref _playerCards);

                if (playerTotal >= 21)
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

            await _message.ModifyAsync(x => x.Embed = BuildEmbed());
            _ = _message.RemoveReactionAsync(emote, context.User);

            _semaphore.Release();
            return false;
        }

        public async Task EndAsync()
        {
            _ = _message.RemoveAllReactionsAsync();
            _interactive.RemoveReactionCallback(_message);

            if (_finished)
                return;

            var playerTotal = CalculateTotal(ref _playerCards);
            var dealerTotal = CalculateTotal(ref _dealerCards);

            _games.LeaveGame(context.User.Id);
            _finished = true;

            if (playerTotal > 21)
            {
                await LoseAsync();
                return;
            }

            while (dealerTotal < 17)
            {
                _dealerCards.Add(_deck.Dequeue());
                dealerTotal = CalculateTotal(ref _dealerCards);
            }

            if (dealerTotal > 21)
            {
                await WinAsync();
            }
            else if (dealerTotal == playerTotal)
            {
                await DrawAsync();
            }
            else if (dealerTotal > playerTotal)
            {
                await LoseAsync();
            }
            else
            {
                await WinAsync();
            }
        }

        private async Task LoseAsync()
        {
            await _candy.UpdateCandiesAsync(context.User.Id, false, -_bet);

            await _message.ModifyAsync(x =>
            {
                x.Embed = new EmbedBuilder
                {
                    Title = "Blackjack Result",
                    Description = $"I win! You lose {_bet}{EmotesHelper.Emotes["rarecandy"]} candies!",
                    Color = Colour.Red,
                    Fields = GetEmbedFields()
                }
                .Build();
            });
        }

        private async Task WinAsync()
        {
            await _candy.UpdateCandiesAsync(context.User.Id, false, _bet);

            await _message.ModifyAsync(x =>
            {
                x.Embed = new EmbedBuilder
                {
                    Title = "Blackjack Result",
                    Description = $"You win! You win {_bet}{EmotesHelper.Emotes["rarecandy"]} candies!",
                    Color = Colour.Green,
                    Fields = GetEmbedFields()
                }
                .Build();
            });
        }

        private async Task DrawAsync()
        {
            await _message.ModifyAsync(x =>
            {
                x.Embed = new EmbedBuilder
                {
                    Title = "Blackjack Result",
                    Description = "It's a draw!",
                    Color = Colour.Orange,
                    Fields = GetEmbedFields()
                }
                .Build();
            });
        }

        private Embed BuildEmbed()
        {
            var builder = new EmbedBuilder
            {
                Title = "Blackjack",
                Description = $"You have bet {_bet}{EmotesHelper.Emotes["rarecandy"]} candies\n" +
                              $"A game of blackjack. Click {_hit} to hit or {_stop} to stay\n",
                Color = Colour.Default,
                Fields = GetEmbedFields()
            };

            return builder.Build();
        }

        private List<EmbedFieldBuilder> GetEmbedFields()
        {
            var fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = $"{context.User.GetDisplayName()}'s cards",
                    Value = $"{GetCards(_playerCards)}\n" +
                        $"For a total of: {CalculateTotal(ref _playerCards)}"
                },

                new EmbedFieldBuilder
                {
                    Name = $"Espeon's cards",
                    Value = $"{GetCards(_dealerCards)}\n" +
                        $"For a total of: {CalculateTotal(ref _dealerCards)}"
                }
            };

            return fields;
        }

        private static int CalculateTotal(ref List<(string suit, string card, int value)> cards)
        {
            var total = cards.Sum(x => x.value);

            if (total <= 21)
                return total;

            if (cards.All(x => x.card != "ace"))
                return total;

            var attemps = cards.Count(x => x.card == "ace");

            for (var a = 0; a < attemps; a++)
                for (var i = 0; i < cards.Count; i++)
                {
                    if (cards[i].card != "ace" || cards[i].value == 1) continue;
                    cards[i] = (cards[i].suit, cards[i].card, 1);
                    break;
                }

            return cards.Sum(x => x.value);
        }

        private static string GetCards(IEnumerable<(string suit, string card, int value)> cards)
        {
            return string.Join(", ", cards.Select(x => $"[{x.card} of {x.suit}]"));
        }
    }
}
