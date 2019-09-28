/*
using Casino.Common;
using Casino.DependencyInjection;
using Discord.WebSocket;
using Espeon.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace Espeon.Bot.Commands
{
    public class InteractiveService2 : BaseService<InitialiseArgs>, IInteractiveService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly TaskQueue _taskQueue;

        private readonly TimeSpan _defaultTimeout;

        public InteractiveService2(IServiceProvider services, TimeSpan? defaultTimeout = null) : base(services)
        {
            _defaultTimeout = defaultTimeout ?? TimeSpan.FromMinutes(2);
        }

        public async Task<SocketMessage> WaitForMessageAsync(Predicate<SocketMessage> predicate,
            TimeSpan? timeout = null, CancellationToken? token = null)
        {
            if(predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            var time = timeout ?? _defaultTimeout;
            var ct = token ?? CancellationToken.None;
            var tcs = new TaskCompletionSource<SocketMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

            Task MessageReceivedAsync(SocketMessage msg)
            {
                if(predicate(msg))
                    tcs.SetResult(msg);

                return Task.CompletedTask;                
            }

            _client.MessageReceived += MessageReceivedAsync;

            var task = await Task.WhenAny(tcs.Task, Task.Delay(time, ct))
                .ConfigureAwait(false);

            _client.MessageReceived -= MessageReceivedAsync;

            return task == tcs.Task ? await tcs.Task.ConfigureAwait(false) : null;
        }

        public async Task<SocketReaction> WaitForReactionAsync(
            Predicate<(Cacheable<IUserMessage, ulong>, IMessageChannel, SocketReaction)> predicate,
            TimeSpan? timeout = null, CancellationToken? token = null)
        {
            if(predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            var time = timeout ?? _defaultTimeout;
            var ct = token ?? CancellationToken.None;
            var tcs = new TaskCompletionSource<SocketReaction>(TaskCreationOptions.RunContinuationsAsynchronously);

            Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheable, IMessageChannel channel, SocketReaction reaction)
            {
                if (predicate((cacheable, channel, reaction)))
                    tcs.SetResult(reaction);

                return Task.CompletedTask;
            }

            _client.ReactionAdded += ReactionAddedAsync;

            var task = await Task.WhenAny(tcs.Task, Task.Delay(time, ct))
                .ConfigureAwait(false);

            _client.ReactionAdded -= ReactionAddedAsync;

            return task == tcs.Task ? await tcs.Task.ConfigureAwait(false) : null;
        }
    }
}
*/
