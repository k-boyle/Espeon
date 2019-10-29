using Casino.Common;
using Casino.DependencyInjection;
using Disqord;
using Disqord.Events;
using Disqord.Rest;
using Espeon.Commands;
using Espeon.Core;
using Espeon.Core.Services;
using Espeon.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon {
	public class InteractiveService : BaseService<InitialiseArgs>,
	                                  IInteractiveService<IReactionCallback, EspeonContext> {
		[Inject] private readonly DiscordClient _client;
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

		async Task<CachedUserMessage> IInteractiveService<IReactionCallback, EspeonContext>.NextMessageAsync(
			EspeonContext context, Func<CachedUserMessage, ValueTask<bool>> predicate, TimeSpan? timeout) {
			timeout ??= DefaultTimeout;

			var taskCompletionSource = new TaskCompletionSource<CachedUserMessage>();

			async Task MessageReceivedAsync(CachedUserMessage message) {
				bool result = await predicate.Invoke(message);

				if (result) {
					taskCompletionSource.SetResult(message);
				}
			}

			Task HandleMessageAsync(MessageReceivedEventArgs args) {
				return args.Message is CachedUserMessage message ? MessageReceivedAsync(message) : Task.CompletedTask;
			}

			this._client.MessageReceived += HandleMessageAsync;

			Task<CachedUserMessage> resultTask = taskCompletionSource.Task;
			Task delay = Task.Delay(timeout.Value);

			Task taskResult = await Task.WhenAny(resultTask, delay);

			this._client.MessageReceived -= HandleMessageAsync;

			return taskResult == resultTask ? await resultTask : null;
		}

		async Task<bool> IInteractiveService<IReactionCallback, EspeonContext>.TryAddCallbackAsync(
			IReactionCallback callback, TimeSpan? timeout) {
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

			foreach (IEmoji emote in callback.Reactions) {
				await message.AddReactionAsync(emote);
			}

			var callbackData = new CallbackData(callback, timeout);

			callbackData.Task = this._scheduler.ScheduleTask(callbackData, timeout, RemoveAsync);

			return this._reactionCallbacks.TryAdd(message.Id, callbackData);
		}

		bool IInteractiveService<IReactionCallback, EspeonContext>.TryRemoveCallback(IReactionCallback callback) {
			if (!this._reactionCallbacks.TryGetValue(callback.Message.Id, out CallbackData callbackData)) {
				return false;
			}

			callbackData.Task.Cancel();

			return this._reactionCallbacks.TryRemove(callback.Message.Id, out _);
		}

		private async Task HandleReactionAsync(ReactionAddedEventArgs args) {
			IMessage message =
				await args.Message.GetOrDownloadAsync<IMessage, CachedMessage, RestMessage>();

			if (message is null) {
				return;
			}

			if (!this._reactionCallbacks.TryGetValue(message.Id, out CallbackData callbackData)) {
				return;
			}

			IReactionCallback callback = callbackData.Callback;
			ICriterion<ReactionAddedEventArgs> criterion = callback.Criterion;
			bool result = await criterion.JudgeAsync(callback.Context, args);

			if (!result) {
				return;
			}

			if (callback.RunOnGatewayThread) {
				await HandleReactionAsync(callbackData, args);
			} else {
				_ = HandleReactionAsync(callbackData, args);
			}
		}

		private async Task HandleReactionAsync(CallbackData data, ReactionAddedEventArgs args) {
			bool result = await data.Callback.HandleCallbackAsync(args);

			if (!result) {
				data.Task.Change(data.Timeout);
			} else {
				data.Task.Cancel();
				await RemoveAsync(data);
			}
		}

		private Task HandleDeletedAsync(MessageDeletedEventArgs args) {
			if (this._reactionCallbacks.TryRemove(args.Message.Id, out CallbackData data)) {
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