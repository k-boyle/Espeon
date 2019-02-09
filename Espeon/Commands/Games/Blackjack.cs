using Discord;
using Discord.WebSocket;
using Espeon.Attributes;
using Espeon.Interactive;
using Espeon.Interactive.Criteria;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.Games
{
    public class Blackjack : IGame
    {
        private const float NormalPayout = 1f;
        private const float BlackjackPayout = 1.5f;

        public EspeonContext Context { get; }
        public IUserMessage Message { get; private set; }
        public IEnumerable<IEmote> Reactions => new[] { _hit, _stop };
        public ICriterion<SocketReaction> Criterion { get; }

        [Inject] private readonly CandyService _candy;
        [Inject] private readonly EmotesService _emotes;
        [Inject] private readonly GamesService _games;
        [Inject] private readonly MessageService _message;

        private Emote RareCandy => _emotes.Collection["RareCandy"];

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
        private readonly bool _manageMessages;

        public Blackjack(EspeonContext context, IServiceProvider services, int bet)
        {
            Context = context;
            Criterion = new ReactionFromSourceUser(context.User.Id);
            
            _deck = new Queue<(string, string, int)>((from suit in _suits from card in _cards select (suit, card.Key, card.Value))
                .OrderBy(_ => services.GetService<Random>().Next()));
            _playerCards = new List<(string suit, string card, int value)>();
            _dealerCards = new List<(string suit, string card, int value)>();

            _bet = bet;
            _manageMessages = Context.Guild.CurrentUser.GetPermissions(Context.Channel).ManageMessages;
        }

        async Task IGame.StartAsync()
        {
            _playerCards.Add(_deck.Dequeue());
            _dealerCards.Add(_deck.Dequeue());
            _playerCards.Add(_deck.Dequeue());

            Message = await _message.SendMessageAsync(Context, string.Empty, embed: BuildEmbed());

            var playerTotal = CalculateTotal(ref _playerCards);

            if (playerTotal == 21)
            {
                await _games.TryLeaveGameAsync(Context);
            }
        }

        async Task IGame.EndAsync()
        {
            if (_manageMessages)
            {
                await Message.RemoveAllReactionsAsync();
            }

            var playerTotal = CalculateTotal(ref _playerCards);
            var dealerTotal = CalculateTotal(ref _dealerCards);

            string description;
            Color color;

            if (playerTotal > 21)
            {
                //lose 

                await _candy.UpdateCandiesAsync(Context, Context.User.Id, -_bet);
                description = $"I win! You lose {_bet}{RareCandy} candies!";
                color = Color.Red;
            }
            else
            {
                while (dealerTotal < 17)
                {
                    _dealerCards.Add(_deck.Dequeue());
                    dealerTotal = CalculateTotal(ref _dealerCards);
                }

                int payout;

                if (dealerTotal > 21)
                {
                    //win

                    payout = (int)(_bet * NormalPayout);

                    await _candy.UpdateCandiesAsync(Context, Context.User.Id, (payout));
                    description = $"I struck out! You win {payout}{RareCandy} candies!";
                    color = Color.Green;
                }
                else if (dealerTotal == playerTotal)
                {
                    //draw

                    description = "It's a draw!";
                    color = Color.Orange;
                }
                else if (dealerTotal > playerTotal)
                {
                    //lose

                    await _candy.UpdateCandiesAsync(Context, Context.User.Id, -_bet);
                    description = $"I win! You lose {_bet}{RareCandy} candies!";
                    color = Color.Red;
                }
                else if(playerTotal == 21)
                {
                    //win 21

                    payout = (int)(_bet * BlackjackPayout);

                    await _candy.UpdateCandiesAsync(Context, Context.User.Id, payout);
                    description = $"BLACKJACK! You win {payout}{RareCandy} candies!";
                    color = Color.Gold;
                }
                else
                {
                    //win

                    payout = (int)(_bet * NormalPayout);

                    await _candy.UpdateCandiesAsync(Context, Context.User.Id, (payout));
                    description = $"You have the higher score! You win {payout}{RareCandy} candies!";
                    color = Color.Green;
                }
            }

            var builder = new EmbedBuilder
            {
                Title = "Blackjack Result!",
                Fields = GetEmbedFields(),
                Description = description,
                Color = color
            };

            await Message.ModifyAsync(x => x.Embed = builder.Build());
        }

        //TODO don't like => Rethink game system?
        Task IReactionCallback.InitialiseAsync()
            => ((IGame)this).StartAsync();

        Task IReactionCallback.HandleTimeoutAsync()
            => _games.TryLeaveGameAsync(Context);

        async Task<bool> IReactionCallback.HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(_hit))
            {
                _playerCards.Add(_deck.Dequeue());
                var playerTotal = CalculateTotal(ref _playerCards);

                if (playerTotal >= 21)
                {
                    await _games.TryLeaveGameAsync(Context);
                    return true;
                }
            }

            if (emote.Equals(_stop))
            {
                await _games.TryLeaveGameAsync(Context);
                return true;
            }

            await Message.ModifyAsync(x => x.Embed = BuildEmbed());

            if (!_manageMessages)
                return false;

            var user = reaction.User.GetValueOrDefault() ?? await Context.Client.Rest.GetUserAsync(reaction.UserId);
            await Message.RemoveReactionAsync(emote, user);

            return false;
        }

        private Embed BuildEmbed()
        {
            var builder = new EmbedBuilder
            {
                Title = "Blackjack",
                Description = $"You have bet {_bet}{RareCandy} candies\n" + 
                              $"A game of blackjack. Click {_hit} to hit or {_stop} to stay\n",
                Color = Color.Default,
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
                    Name = $"{Context.User.GetDisplayName()}'s cards",
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
