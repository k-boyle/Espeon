using Casino.Common;
using Casino.DependencyInjection;
using Casino.Linq;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using Espeon.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class MessageService : BaseService<InitialiseArgs>, IMessageService {
		private static readonly TimeSpan MessageLifeTime = TimeSpan.FromMinutes(10);

		//[(channel_id, user_id, message_id)]
		private readonly ConcurrentDictionary<(ulong, ulong, ulong), ScheduledTask<ConcurrentQueue<CachedMessage>>>
			_messageCache;

		private readonly ConcurrentDictionary<ulong, DateTimeOffset> _editCache;

		[Inject] private readonly TaskQueue _scheduler;

		public MessageService(IServiceProvider services) : base(services) {
			this._messageCache =
				new ConcurrentDictionary<(ulong, ulong, ulong), ScheduledTask<ConcurrentQueue<CachedMessage>>>(2, 20);
			this._editCache = new ConcurrentDictionary<ulong, DateTimeOffset>(2, 20);
		}

		//https://drive.google.com/file/d/1ntt12p1i_2B1h3MXUgtXuL1gi3AUd-jL/view outdated lol but effort to update, still has the right idea
		async Task<RestUserMessage> IMessageService.SendAsync(SocketUserMessage sourceMessage,
			Action<NewMessageProperties> properties) {
			(ulong, ulong, ulong) key = (sourceMessage.Channel.Id, sourceMessage.Author.Id, sourceMessage.Id);
			NewMessageProperties props = properties.Invoke();

			bool alreadyScheduled =
				this._messageCache.TryGetValue(key, out ScheduledTask<ConcurrentQueue<CachedMessage>> scheduled);

			if (sourceMessage.EditedTimestamp.HasValue) {
				DateTimeOffset timeStamp = sourceMessage.EditedTimestamp.Value;

				static async Task DeleteMessagesAsync(SocketTextChannel channel, SocketGuild guild,
					CachedMessage[] cachedMessages) {
					if (guild.CurrentUser.GetPermissions(channel).ManageMessages) {
						IMessage[] fetchedMessages = await cachedMessages
							.Select(x => channel.GetMessageAsync(x.ResponseId)).AllAsync();

						await channel.DeleteMessagesAsync(fetchedMessages.OfType<IUserMessage>());
					} else {
						for (var i = 0; i < cachedMessages.Length; i++) {
							IMessage fetched = await channel.GetMessageAsync(cachedMessages[i].ResponseId);

							if (fetched is null) {
								continue;
							}

							await fetched.DeleteAsync();
						}
					}
				}

				if (this._editCache.TryGetValue(sourceMessage.Id, out DateTimeOffset editedAt) &&
				    editedAt != timeStamp || alreadyScheduled) {
					CachedMessage[] fromSource = scheduled.State.ToArray(x => x.ExecutingId == sourceMessage.Id);

					var asTextChannel = sourceMessage.Channel as SocketTextChannel;
					await DeleteMessagesAsync(asTextChannel, asTextChannel.Guild, fromSource);
				}

				this._editCache.AddOrUpdate(sourceMessage.Id, timeStamp, (_, __) => timeStamp);
			}

			bool attachment = props.Stream is null;

			RestUserMessage message = attachment
				? await sourceMessage.Channel.SendMessageAsync(props.Content, props.IsTTS, props.Embed)
				: await sourceMessage.Channel.SendFileAsync(props.Stream, props.FileName, props.Content, props.IsTTS,
					props.Embed, isSpoiler: props.IsSpoiler);

			var cached = new CachedMessage(sourceMessage.Channel.Id, sourceMessage.Id, sourceMessage.Author.Id,
				message.Id, attachment, false, message.CreatedAt);

			if (alreadyScheduled) {
				scheduled.State.Enqueue(cached);
			} else {
				var queue = new ConcurrentQueue<CachedMessage>();
				queue.Enqueue(cached);

				scheduled = this._scheduler.ScheduleTask(queue, MessageLifeTime, RemoveCacheAsync);

				this._messageCache.TryAdd(key, scheduled);
			}

			return message;
		}

		private Task RemoveCacheAsync(ConcurrentQueue<CachedMessage> queue) {
			if (queue.TryDequeue(out CachedMessage cached) && queue.IsEmpty &&
			    this._messageCache.TryRemove((cached.ChannelId, cached.UserId, cached.ExecutingId), out _) &&
			    this._editCache.TryRemove(cached.ExecutingId, out _)) { }

			return Task.CompletedTask;
		}

		async Task IMessageService.DeleteMessagesAsync(SocketTextChannel channel, SocketGuildUser bot, ulong userId, int amount) {
			foreach (((ulong channelId, ulong userId_, _), ScheduledTask<ConcurrentQueue<CachedMessage>> value) in this
				._messageCache) {
				if (!(channelId == channel.Id && userId_ == userId)) {
					continue;
				}

				bool manageMesssages = bot.GetPermissions(channel).ManageMessages;
				var messages = new List<IMessage>();

				for (; amount > 0 && value.State.TryDequeue(out CachedMessage cached);) {
					IMessage message = await channel.GetMessageAsync(cached.ResponseId);

					if (message is null) {
						continue;
					}

					if (manageMesssages) {
						messages.Add(message);
					} else {
						await message.DeleteAsync();
					}

					amount--;
				}

				if (messages.Count > 0) {
					await channel.DeleteMessagesAsync(messages);
				}
			}
		}
	}
}