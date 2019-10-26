using Casino.Common;
using Casino.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Espeon.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class InteractiveService : BaseService<InitialiseArgs>, IInteractiveService {
		[Inject] private readonly DiscordSocketClient _client;
		[Inject] private readonly TaskQueue _scheduler;

		private readonly ConcurrentDictionary<ulong, CallbackData> _reactionCallbacks;

		private static TimeSpan DefaultTimeout => TimeSpan.FromMinutes(2);

		public InteractiveService(IServiceProvider services) : base(services) {
			this._reactionCallbacks = new ConcurrentDictionary<ulong, CallbackData>();
		}

		public override Task InitialiseAsync(IServiceProvider services, InitialiseArgs args) {
			this._client.ReactionAdded += HandleReactionAsync;
			this._client.MessageDeleted += HandleDeletedAsync;
			return Task.CompletedTask;
		}

		async Task<SocketUserMessage> IInteractiveService.NextMessageAsync(EspeonContext context,
			ICriterion<SocketUserMessage> criterion, TimeSpan? timeout) {
			timeout??=DefaultTimeout;

			var taskCompletionSource = new TaskCompletionSource<SocketUserMessage>();

			async Task MessageReceivedAsync(SocketUserMessage message) {
				bool result = await criterion.JudgeAsync(context, message);

				if (result) {
					taskCompletionSource.SetResult(message);
				}
			}

			Task HandleMessageAsync(SocketMessage msg) {
				return msg is SocketUserMessage message ? MessageReceivedAsync(message) : Task.CompletedTask;
			}

			context.Client.MessageReceived += HandleMessageAsync;

			Task<SocketUserMessage> resultTask = taskCompletionSource.Task;
			Task delay = Task.Delay(timeout.Value);

			Task taskResult = await Task.WhenAny(resultTask, delay);

			context.Client.MessageReceived -= HandleMessageAsync;

			return taskResult == resultTask ? await resultTask : null;
		}

		async Task<bool> IInteractiveService.TryAddCallbackAsync(IReactionCallback callback, TimeSpan? timeout) {
			IUserMessage message = callback.Message;

			//null check for games support... Yeah I should rethink this
			if (!(message is null) && this._reactionCallbacks.ContainsKey(message.Id)) {
				return false;
			}

			timeout??=DefaultTimeout;

			await callback.InitialiseAsync();

			return await InternalAddCallbackAsync(callback, timeout.Value);
		}

		private async Task<bool> InternalAddCallbackAsync(IReactionCallback callback, TimeSpan timeout) {
			IUserMessage message = callback.Message;

			foreach (IEmote emote in callback.Reactions) {
				await message.AddReactionAsync(emote);
			}

			var callbackData = new CallbackData(callback, timeout);

			callbackData.Task = this._scheduler.ScheduleTask(callbackData, timeout, RemoveAsync);

			return this._reactionCallbacks.TryAdd(message.Id, callbackData);
		}

		bool IInteractiveService.TryRemoveCallback(IReactionCallback callback) {
			if (!this._reactionCallbacks.TryGetValue(callback.Message.Id, out CallbackData callbackData)) {
				return false;
			}

			callbackData.Task.Cancel();

			return this._reactionCallbacks.TryRemove(callback.Message.Id, out _);
		}

		private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cachedMessage,
			ISocketMessageChannel channel, SocketReaction reaction) {
			IUserMessage message = await cachedMessage.GetOrDownloadAsync();

			if (message is null) {
				return;
			}

			if (!this._reactionCallbacks.TryGetValue(message.Id, out CallbackData callbackData)) {
				return;
			}

			IReactionCallback callback = callbackData.Callback;
			ICriterion<SocketReaction> criterion = callback.Criterion;
			bool result = await criterion.JudgeAsync(callback.Context, reaction);

			if (!result) {
				return;
			}

			if (callback.RunOnGatewayThread) {
				await HandleReactionAsync(callbackData, reaction);
			} else {
				_ = HandleReactionAsync(callbackData, reaction);
			}
		}

		private async Task HandleReactionAsync(CallbackData data, SocketReaction reaction) {
			bool result = await data.Callback.HandleCallbackAsync(reaction);

			if (!result) {
				data.Task.Change(data.Timeout);
			} else {
				data.Task.Cancel();
				await RemoveAsync(data);
			}
		}

		private Task HandleDeletedAsync(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel) {
			if (this._reactionCallbacks.TryRemove(cache.Id, out CallbackData data)) {
				data.Task.Cancel();
			}

			return Task.CompletedTask;
		}

		private async Task RemoveAsync(CallbackData callbackData) {
			IReactionCallback callback = callbackData.Callback;
			await callback.HandleTimeoutAsync();

			this._reactionCallbacks.TryRemove(callback.Message.Id, out _);
		}

		private class CallbackData {
			public IReactionCallback Callback { get; }
			public TimeSpan Timeout { get; }

			//public long WhenToRemove { get; set; }

			public ScheduledTask<CallbackData> Task { get; set; }

			public CallbackData(IReactionCallback callback, TimeSpan timeout) {
				Callback = callback;
				Timeout = timeout;
			}
		}
	}
}