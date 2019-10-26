using Casino.Common;
using Casino.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class QuoteService : BaseService<InitialiseArgs>, IQuoteService {
		private const string RegexString = @"(?:https://(?:canary.)?discordapp.com/channels/[\d]+/[\d]+/[\d]+)";

		private static readonly Regex Regex = new Regex(RegexString, RegexOptions.Compiled);
		private static readonly Emoji QuoteEmote = new Emoji("🗨");
		private static readonly TimeSpan MessageLifeTime = TimeSpan.FromMinutes(10);

		private readonly ConcurrentDictionary<ulong, ulong> _lastJumpUrlQuotes;
		private readonly ConcurrentQueue<ulong> _quoteReactions;

		[Inject] private readonly DiscordSocketClient _client;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly TaskQueue _scheduler;

		public QuoteService(IServiceProvider services) : base(services) {
			this._lastJumpUrlQuotes = new ConcurrentDictionary<ulong, ulong>(2, 10);
			this._quoteReactions = new ConcurrentQueue<ulong>();

			this._client.MessageReceived += msg => this._events.RegisterEvent(() => {
				if (msg.Channel is IDMChannel) {
					return Task.CompletedTask;
				}

				CacheJumpUrl(msg);
				return Task.CompletedTask;
			});

			this._client.ReactionAdded += HandleReactionAsync;
		}

		private void CacheJumpUrl(SocketMessage message) {
			Match match = Regex.Match(message.Content);

			if (match.Success) {
				this._lastJumpUrlQuotes[message.Channel.Id] = message.Id;
			}
		}

		private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel,
			SocketReaction reaction) {
			if (!reaction.Emote.Equals(QuoteEmote) || this._quoteReactions.Any(x => x == cache.Id) ||
			    channel is IDMChannel) {
				return;
			}

			IUserMessage message = await cache.GetOrDownloadAsync();

			if (message is null) {
				return;
			}

			Embed embed = await Utilities.QuoteFromStringAsync(this._client, message.Content);

			if (embed is null) {
				return;
			}

			await message.Channel.SendMessageAsync(string.Empty, embed: embed);

			this._quoteReactions.Enqueue(cache.Id);

			this._scheduler.ScheduleTask(this._quoteReactions, MessageLifeTime, state => {
				state.TryDequeue(out _);
				return Task.CompletedTask;
			});
		}

		bool IQuoteService.TryGetLastJumpMessage(ulong channelId, out ulong messageId) {
			return this._lastJumpUrlQuotes.TryGetValue(channelId, out messageId);
		}
	}
}