using Casino.Common;
using Casino.DependencyInjection;
using Casino.Linq;
using Discord;
using Discord.Rest;
using Espeon.Core;
using Espeon.Core.Commands;
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
		async Task<RestUserMessage> IMessageService.SendAsync(EspeonContext context,
			Action<NewMessageProperties> properties) {
			(ulong, ulong, ulong) key = (context.Channel.Id, context.User.Id, context.Message.Id);
			NewMessageProperties props = properties.Invoke();

			bool alreadyScheduled =
				this._messageCache.TryGetValue(key, out ScheduledTask<ConcurrentQueue<CachedMessage>> scheduled);

			if (context.Message.EditedTimestamp.HasValue) {
				DateTimeOffset timeStamp = context.Message.EditedTimestamp.Value;

				static async Task DeleteMessagesAsync(EspeonContext context, CachedMessage[] cachedMessages) {
					if (context.Guild.CurrentUser.GetPermissions(context.Channel).ManageMessages) {
						IMessage[] fetchedMessages = await cachedMessages
							.Select(x => context.Channel.GetMessageAsync(x.ResponseId)).AllAsync();

						await context.Channel.DeleteMessagesAsync(fetchedMessages.OfType<IUserMessage>());
					} else {
						for (var i = 0; i < cachedMessages.Length; i++) {
							IMessage fetched = await context.Channel.GetMessageAsync(cachedMessages[i].ResponseId);

							if (fetched is null) {
								continue;
							}

							await fetched.DeleteAsync();
						}
					}
				}

				if (this._editCache.TryGetValue(context.Message.Id, out DateTimeOffset editedAt) &&
				    editedAt != timeStamp || alreadyScheduled) {
					CachedMessage[] fromSource = scheduled.State.ToArray(x => x.ExecutingId == context.Message.Id);

					await DeleteMessagesAsync(context, fromSource);
				}

				this._editCache.AddOrUpdate(context.Message.Id, timeStamp, (_, __) => timeStamp);
			}

			bool attachment = props.Stream is null;

			RestUserMessage message = attachment
				? await context.Channel.SendMessageAsync(props.Content, props.IsTTS, props.Embed)
				: await context.Channel.SendFileAsync(props.Stream, props.FileName, props.Content, props.IsTTS,
					props.Embed, isSpoiler: props.IsSpoiler);

			var cached = new CachedMessage(context.Channel.Id, context.Message.Id, context.User.Id, message.Id,
				attachment, false, message.CreatedAt);

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

		async Task IMessageService.DeleteMessagesAsync(EspeonContext context, int amount) {
			foreach (((ulong channelId, ulong userId, _), ScheduledTask<ConcurrentQueue<CachedMessage>> value) in this
				._messageCache) {
				if (!(channelId == context.Channel.Id && userId == context.User.Id)) {
					continue;
				}

				bool manageMesssages = context.Guild.CurrentUser.GetPermissions(context.Channel).ManageMessages;
				var messages = new List<IMessage>();

				for (; amount > 0 && value.State.TryDequeue(out CachedMessage cached);) {
					IMessage message = await context.Channel.GetMessageAsync(cached.ResponseId);

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
					await context.Channel.DeleteMessagesAsync(messages);
				}
			}
		}
	}
}