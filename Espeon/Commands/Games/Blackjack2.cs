using Casino.DependencyInjection;
using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class Blackjack2 : IGame {
		private const float NormalPayout = 2f;
		private const float BlackjackPayout = 2.5f;
		private const float InsurancePayout = 3.0f;

		private readonly IReadOnlyDictionary<string, int> _cards = new Dictionary<string, int> {
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

		private readonly IReadOnlyCollection<string> _suits = new[] {
			"❤",
			"♦",
			"♣",
			"♠"
		};

		private Queue<(string suit, string card, int value)> _deck;

		private readonly List<(List<(string Suit, string Card, int Value)> Cards, int Bet)> _playerHands;
		private readonly List<(string Suit, string Card, int Value)> _dealerHand;

		private int _hands = 1;
		private int _currentHand = 0;

		private readonly int _bet;

		[Inject] private readonly IEmoteService _emotes;
		[Inject] private readonly IGamesService _games;
		[Inject] private readonly IMessageService _message;
		[Inject] private readonly Random _random;

		private Emote _candyEmote => this._emotes.Collection["RareCandy"];
		private readonly Emoji _hit = new Emoji("➕");
		private readonly Emoji _stop = new Emoji("❌");
		private readonly Emoji _double = new Emoji("\u0032\u20e3");
		private readonly Emoji _split = new Emoji("↔");
		private readonly Emoji _insurance = new Emoji("ℹ");

		public EspeonContext Context { get; }
		public bool RunOnGatewayThread => true;
		public IUserMessage Message { get; private set; }
		public IEnumerable<IEmote> Reactions => new Emoji[] { };
		public ICriterion<SocketReaction> Criterion { get; }

		private readonly bool _manageMessage;
		private bool _canInsurance;
		private bool _canSplit;
		private bool _canDouble;
		private bool _added;

		private readonly Dictionary<int, Result> _results;

		public Blackjack2(int bet, EspeonContext context, IServiceProvider services) {
			this._bet = bet;
			Context = context;
			services.Inject(this);

			Criterion = new ReactionFromSourceUser(context.User.Id);

			InitialiseDeck();

			this._playerHands = new List<(List<(string Suit, string Card, int Value)> Cards, int Bet)>();
			this._dealerHand = new List<(string Suit, string Card, int Value)>();

			this._manageMessage = Context.Guild.CurrentUser.GetPermissions(Context.Channel).ManageMessages;

			this._results = new Dictionary<int, Result>();
		}

		private void InitialiseDeck() {
			this._deck = new Queue<(string, string, int)>(
				(from suit in this._suits from card in this._cards select (suit, card.Key, card.Value)).OrderBy(_ =>
					this._random.Next()));
		}

		private (string, string, int) DrawCard() {
			return this._deck.Dequeue();
		}

		private static int Total(List<(string Suit, string Card, int Value)> hand) {
			int total = hand.Sum(x => x.Value);

			if (total <= 21) {
				return total;
			}

			if (hand.All(x => x.Card != "ace")) {
				return total;
			}

			int index = hand.FindIndex(x => x.Value == 11);
			while (index > -1) {
				(string suit, string card, _) = hand[index];

				hand[index] = (suit, card, 1);

				index = hand.FindIndex(x => x.Value == 11);
			}

			return hand.Sum(x => x.Value);
		}

		private Result Hit(List<(string Suit, string Card, int Value)> hand) {
			hand.Add(DrawCard());

			int total = Total(hand);

			if (total == 21) {
				return Result.Blackjack;
			}

			return total > 21 ? Result.StruckOut : Result.None;
		}

		private enum Result {
			Blackjack,
			StruckOut,
			None
		}

		public Task InitialiseAsync() {
			return Task.CompletedTask;
		}

		public Task HandleTimeoutAsync() {
			return this._games.TryLeaveGameAsync(Context);
		}

		public async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
			IEmote emote = reaction.Emote;

			if (this._manageMessage) {
				IUser user = await Context.Client.GetOrFetchUserAsync(reaction.UserId);

				await Message.RemoveReactionAsync(emote, user);
			}

			(List<(string Suit, string Card, int Value)> cards, _) = this._playerHands[this._currentHand];

			if (emote.Equals(this._hit)) {
				Result result = Hit(cards);

				if (result != Result.None) {
					this._results.Add(this._currentHand++, result);

					if (this._currentHand == this._hands) {
						await Message.ModifyAsync(x => x.Embed = GetEmbed());

						if (this._manageMessage) {
							await Message.RemoveAllReactionsAsync();
						}

						return true;
					}

				}
			} else if (emote.Equals(this._stop)) { } else if (emote.Equals(this._double) && this._canDouble) { } else if
				(emote.Equals(this._insurance) && this._canInsurance) { } else if (emote.Equals(this._split) &&
				                                                                   this._canSplit) { }

			if (this._manageMessage) {
				await Message.RemoveReactionAsync(this._double, Context.Client.CurrentUser);
				await Message.RemoveReactionAsync(this._insurance, Context.Client.CurrentUser);
				await Message.RemoveReactionAsync(this._split, Context.Client.CurrentUser);
				await AddReactionsAsync();
			}

			await Message.ModifyAsync(x => x.Embed = GetEmbed());

			return false;
		}

		public async Task<bool> StartAsync() {
			this._playerHands.Add((new List<(string suit, string card, int value)>(), this._bet));
			(List<(string Suit, string Card, int Value)> cards, _) = this._playerHands[0];

			Hit(cards);
			Hit(this._dealerHand);
			Result result = Hit(cards);

			if (this._dealerHand[0].Card == "ace") {
				this._canInsurance = true;
			}

			if (cards[0].Card == cards[1].Card) {
				this._canSplit = true;
			}

			int total = Total(cards);

			if (total == 9 || total == 10 || total == 11) {
				this._canDouble = true;
			}

			Message = await this._message.SendAsync(Context, x => x.Embed = GetEmbed());

			if (result == Result.Blackjack) {
				return true;
			}

			await AddReactionsAsync();

			return false;
		}

		private async Task AddReactionsAsync() {
			if (!this._added) {
				await Message.AddReactionsAsync(new IEmote[] {
					this._hit,
					this._stop
				});
				this._added = true;
			}

			if (this._canInsurance) {
				await Message.AddReactionAsync(this._insurance);
			}

			if (this._canSplit) {
				await Message.AddReactionAsync(this._split);
			}

			if (this._canDouble) {
				await Message.AddReactionAsync(this._double);
			}
		}

		public async Task EndAsync() { }

		private Embed GetEmbed() {
			string GetCards(IEnumerable<(string suit, string card, int value)> cards) {
				return string.Join(", ", cards.Select(x => $"[{x.card} of {x.suit}]"));
			}

			List<EmbedFieldBuilder> GetFields() {
				var toReturn = new List<EmbedFieldBuilder> {
					new EmbedFieldBuilder {
						Name = $"Dealers Hand. Total: {Total(this._dealerHand)}",
						Value = GetCards(this._dealerHand)
					}
				};

				toReturn.AddRange(this._playerHands.Select((hand, index) => new EmbedFieldBuilder {
					Name =
						$"Your hand {index + 1}/{this._hands}. Bet: {hand.Bet}{_candyEmote} Total: {Total(hand.Cards)}",
					Value = GetCards(hand.Cards)
				}));

				return toReturn;
			}

			var builder = new EmbedBuilder {
				Title = "Blackjack",
				Color = Color.Default,
				Fields = GetFields()
			};

			var sb = new StringBuilder();

			sb.AppendLine("A game of blackjack!");
			sb.AppendLine($"Press {this._hit} to hit!");
			sb.AppendLine($"Press {this._stop} to stay!");

			if (this._canInsurance) {
				sb.AppendLine($"Press {this._insurance} to place an insurance bet!");
			}

			if (this._canSplit) {
				sb.AppendLine($"Press {this._split} to split!");
			}

			if (this._canDouble) {
				sb.AppendLine($"Press {this._double} to double your bet!");
			}

			builder.WithDescription(sb.ToString());

			return builder.Build();
		}
	}
}