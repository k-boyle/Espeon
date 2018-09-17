using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Interactive.Callbacks;
using Espeon.Interactive.Criteria;

namespace Espeon.Interactive
{
    [Service]
    public class InteractiveService : IDisposable
    {
        private DiscordSocketClient Discord { get; }

        private readonly ConcurrentDictionary<ulong, IReactionCallback> _callbacks;
        private readonly TimeSpan _defaultTimeout;

        public InteractiveService(DiscordSocketClient discord, TimeSpan? defaultTimeout = null)
        {
            Discord = discord;
            Discord.ReactionAdded += HandleReactionAsync;

            _callbacks = new ConcurrentDictionary<ulong, IReactionCallback>();
            _defaultTimeout = defaultTimeout ?? TimeSpan.FromSeconds(15);
        }

        public Task<SocketMessage> NextMessageAsync(ICommandContext context, bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null)
        {
            var criterion = new Criteria<SocketMessage>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureSourceUserCriterion());
            if (inSourceChannel)
                criterion.AddCriterion(new EnsureSourceChannelCriterion());
            return NextMessageAsync(context, criterion, timeout);
        }

        public async Task<SocketMessage> NextMessageAsync(ICommandContext context, ICriterion<SocketMessage> criterion, TimeSpan? timeout = null)
        {
            timeout = timeout ?? _defaultTimeout;

            var eventTrigger = new TaskCompletionSource<SocketMessage>();

            async Task Handler(SocketMessage message)
            {
                var result = await criterion.JudgeAsync(context, message).ConfigureAwait(false);
                if (result)
                    eventTrigger.SetResult(message);
            }

            if (!(context.Client is DiscordSocketClient client)) return null;
            client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay).ConfigureAwait(false);

            client.MessageReceived -= Handler;

            if (task == trigger)
                return await trigger.ConfigureAwait(false);
            return null;
        }

        public async Task<IUserMessage> ReplyAndDeleteAsync(ICommandContext context, string content, bool isTTS = false, Embed embed = null, TimeSpan? timeout = null, RequestOptions options = null)
        {
            timeout = timeout ?? _defaultTimeout;
            var message = await context.Channel.SendMessageAsync(content, isTTS, embed, options).ConfigureAwait(false);
            _ = Task.Delay(timeout.Value)
                .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            return message;
        }

        public void AddReactionCallback(IMessage message, IReactionCallback callback)
            => _callbacks[message.Id] = callback;

        public void RemoveReactionCallback(IMessage message)
            => RemoveReactionCallback(message.Id);

        private void RemoveReactionCallback(ulong id)
            => _callbacks.TryRemove(id, out _);

        public void ClearReactionCallbacks()
            => _callbacks.Clear();
        
        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == Discord.CurrentUser.Id) return;
            if (!(_callbacks.TryGetValue(message.Id, out var callback))) return;
            if (!(await callback.Criterion.JudgeAsync(callback.Context, reaction).ConfigureAwait(false)))
                return;
            switch (callback.RunMode)
            {
                case RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                            RemoveReactionCallback(message.Id);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                        RemoveReactionCallback(message.Id);
                    break;
            }
        }

        public void Dispose()
        {
            Discord.ReactionAdded -= HandleReactionAsync;
        }
    }
}
