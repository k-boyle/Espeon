using Disqord;
using Disqord.Events;
using Disqord.Rest;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Database.GuildStore;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class StarboardService : BaseService<InitialiseArgs>, IStarboardService {
		[Inject] private readonly DiscordClient _client;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly IServiceProvider _services;
		[Inject] private readonly ILogService _logger;

		private static LocalEmoji Star => Utilities.Star;

		public StarboardService(IServiceProvider services) : base(services) {
			this._client.ReactionAdded += args =>
				this._events.RegisterEvent(() => ReactionAddedAsync(args));

			this._client.ReactionRemoved += args =>
				this._events.RegisterEvent(() => ReactionRemovedAsync(args));
		}

		private async Task ReactionAddedAsync(ReactionAddedEventArgs args) {
			if (!(args.Channel is CachedTextChannel textChannel)) {
				return;
			}

			if (!args.Emoji.Equals(Star)) {
				return;
			}

			IMessage message = await args.Message.GetOrDownloadAsync<IMessage, CachedMessage, RestMessage>();

			if (args.User.Id == message.Author.Id) {
				return;
			}

			try {
				using var guildStore = this._services.GetService<GuildStore>();
				Guild guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

				if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is { } starChannel)) {
					return;
				}

				int count = message.Reactions[Star].Count;

				IEnumerable<IUser> flat = await message.GetReactionsAsync(Star, count);
				IUser[] users = flat.Where(x => x.Id != message.Author.Id).ToArray();

				count = users.Length;

				if (count < guild.StarLimit) {
					return;
				}

				StarredMessage foundMessage =
					guild.StarredMessages.FirstOrDefault(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

				string m =
					$"{Star} **{count}** - {(message.Author as IMember)?.DisplayName} in <#{message.ChannelId}>";

				if (foundMessage is null) {
					LocalEmbed embed = await Utilities.BuildStarMessageAsync(message);

					RestUserMessage newStar = await starChannel.SendMessageAsync(m, embed: embed);

					var toAdd = new StarredMessage {
						AuthorId = message.Author.Id,
						ChannelId = message.ChannelId,
						Id = message.Id,
						StarboardMessageId = newStar.Id,
						ReactionUsers = users.Select(x => x.Id.RawValue).ToList(),
						ImageUrl = embed.ImageUrl,
						Content = message.Content
					};

					guild.StarredMessages.Add(toAdd);
				} else {
					if (foundMessage.ReactionUsers.Contains(args.User.Id)) {
						return;
					}

					foundMessage.ReactionUsers.Add(args.User.Id);

					if (await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) is IUserMessage
						fetchedMessage) {
						await fetchedMessage.ModifyAsync(x => x.Content = m);
					}
				}

				guildStore.Update(guild);

				await guildStore.SaveChangesAsync();
			} catch (Exception ex) {
				this._logger.Log(Source.Starboard, Severity.Error, string.Empty, ex);
			}
		}

		private async Task ReactionRemovedAsync(ReactionRemovedEventArgs args) {
			if (!(args.Channel is CachedTextChannel textChannel)) {
				return;
			}

			if (!args.Emoji.Equals(Star)) {
				return;
			}

			try {
				using var guildStore = this._services.GetService<GuildStore>();
				Guild guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

				if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is { } starChannel)) {
					return;
				}

				IMessage msg = await args.Message.GetOrDownloadAsync<IMessage, CachedMessage, RestMessage>();

				StarredMessage foundMessage =
					guild.StarredMessages.Find(x => x.Id == msg.Id || x.StarboardMessageId == msg.Id);

				if (foundMessage is null) {
					return;
				}

				if (!foundMessage.ReactionUsers.Remove(args.User.Id)) {
					return;
				}

				int count = msg.Reactions.ContainsKey(Star) ? msg.Reactions[Star].Count : 0;

				var starMessage = await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) as IUserMessage;

				if (starMessage is null || count < guild.StarLimit) {
					_ = starMessage?.DeleteAsync();

					guild.StarredMessages.Remove(foundMessage);
				} else {
					string m =
						$"{Star} **{count}** - {(msg.Author as IMember)?.DisplayName} in <#{msg.ChannelId}>";

					await starMessage.ModifyAsync(x => x.Content = m);
				}

				guildStore.Update(guild);

				await guildStore.SaveChangesAsync();
			} catch (Exception ex) {
				this._logger.Log(Source.Starboard, Severity.Error, string.Empty, ex);
			}
		}
	}
}