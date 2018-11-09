using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using Espeon.Implementation.Entities;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Implementation.Services
{
    [Service(typeof(IMessageService<>), typeof(EspeonContext), true)]
    public class MessageService : IMessageService<EspeonContext>
    {
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IDatabaseService _database;
        [Inject] private readonly IServiceProvider _services;
        [Inject] private readonly ITimerService _timer;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        private readonly ConcurrentDictionary<ulong, IFixedQueue<(Message Message, string Key)>> _messageCache;

        private const int CacheSize = 20;
        private static TimeSpan MessageLifeTime => TimeSpan.FromMinutes(10);

        public MessageService()
        {
            _messageCache = new ConcurrentDictionary<ulong, IFixedQueue<(Message message, string key)>>();
        }

        [Initialiser]
        public void Initialise()
        {
            _commands.CommandErrored += CommandErroredAsync;
            _client.MessageUpdated += (_, msg, __) =>
                msg is SocketUserMessage message ? HandleReceivedMessageAsync(message, true) : Task.CompletedTask;
        }

        Task IMessageService<EspeonContext>.HandleReceivedMessageAsync(SocketMessage msg)
            => msg is SocketUserMessage message ? HandleReceivedMessageAsync(message, false) : Task.CompletedTask;

        private async Task HandleReceivedMessageAsync(SocketUserMessage message, bool isEdit)
        {
            if (message.Author.IsBot && message.Author.Id != _client.CurrentUser.Id ||
                message.Channel is IPrivateChannel) return;

            var context = new EspeonContext(_client, message, isEdit);
            var guild = await _database.GetAndCacheEntityAsync<Guild>("guilds", context.Guild.Id);

            if (guild is null)
            {
                guild = new Guild
                {
                    Id = context.Guild.Id
                };

                await _database.WriteAsync("guilds", guild);
            }

            var prefixes = guild.Config.Prefixes;

            if (CommandUtilities.HasAnyPrefix(message.Content, prefixes, StringComparison.CurrentCulture,
                    out _, out var output) || message.HasMentionPrefix(_client.CurrentUser, out output))
            {
                await _commands.ExecuteAsync(output, context, _services);
            }
        }

        private Task CommandErroredAsync(ExecutionFailedResult result, ICommandContext context, IServiceProvider services)
        {
            return Task.CompletedTask;

        }

        public async Task<IUserMessage> SendMessageAsync(EspeonContext context, string content, Embed embed = null)
        {
            var foundItem = _messageCache[context.User.Id].FirstOrDefault(x => x.Message.ExecutingId == context.Message.Id);

            if (context.IsEdit && !(foundItem.Message is null || string.IsNullOrWhiteSpace(foundItem.Key)))
            {
                //TODO figure out how I'm gonna handle message edits... Probably gonna need to rewrite this... RIP
            }

            var message = await context.Channel.SendMessageAsync(content, embed: embed);

            //stupid c#
            if (foundItem.Message is null || string.IsNullOrWhiteSpace(foundItem.Key))
            {
                var item = new Message
                {
                    ChannelId = context.Channel.Id,
                    ExecutingId = context.Message.Id,
                    UserId = context.User.Id,
                    ResponseIds = new[] { message.Id },
                    WhenToRemove = DateTimeOffset.UtcNow.Add(MessageLifeTime).ToUnixTimeMilliseconds()
                };

                var key = await _timer.EnqueueAsync(item, RemoveAsync);

                var queue = new FixedQueue<(Message message, string key)>(CacheSize);
                queue.TryEnqueue((item, key));

                _messageCache[context.User.Id] = queue;

                return message;
            }

            //TODO check this does what I hope it does even though it's ugly
            _messageCache[context.User.Id].FirstOrDefault(x => x.Message.ExecutingId == context.Message.Id).Message
                .ResponseIds.Add(message.Id);

            return message;
        }

        private Task<IMessage> GetOrDownloadMessageAsync(ulong channelId, ulong messageId)
        {
            if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                return null;

            return !(channel.GetCachedMessage(messageId) is IMessage message)
                ? channel.GetMessageAsync(messageId)
                : Task.FromResult(message);
        }

        private Task RemoveAsync(IRemovable removeable)
        {
            var message = removeable as Message;

            var queue = _messageCache[message.UserId];

            var filtered = queue.Where(item => item.Message.ExecutingId != message.ExecutingId).ToList();

            _messageCache[message.UserId] = new FixedQueue<(Message, string)>(CacheSize, filtered);

            return Task.CompletedTask;
        }
    }
}
