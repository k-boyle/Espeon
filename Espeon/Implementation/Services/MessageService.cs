using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Espeon.Implementation.Entities;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Espeon.Implementation.Services
{
    [Service(typeof(IMessageService), true)]
    public class MessageService : IMessageService
    {
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IDatabaseService _database;
        [Inject] private readonly IServiceProvider _services;
        [Inject] private readonly ITimerService _timer;

        private readonly ConcurrentDictionary<ulong, IFixedQueue<Message>> _messageCache;
        
        private const int CacheSize = 20;
        private static TimeSpan MessageTolerance => TimeSpan.FromSeconds(5);

        public MessageService()
        {
            _messageCache = new ConcurrentDictionary<ulong, IFixedQueue<Message>>();
            _commands.CommandErrored += CommandErroredAsync;
            _client.MessageUpdated += (_, message, __) => HandleReceivedMessageAsync(message);
        }
        public async Task HandleReceivedMessageAsync(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message) ||
                message.Author.IsBot && message.Author.Id != _client.CurrentUser.Id ||
                message.Channel is IPrivateChannel) return;

            var context = new EspeonContext(_client, message);
            var guild = await _database.GetEntityAsync<Guild>("guilds", context.Guild.Id);

            var prefixes = guild.Config.Prefixes;

            if (CommandUtilities.HasAnyPrefix(message.Content, prefixes, StringComparison.CurrentCulture,
                    out _, out var output) || message.HasMentionPrefix(_client.CurrentUser, out output))
            {
                await _commands.ExecuteAsync(output, context, _services);
            }
        }

        private Task CommandErroredAsync(ExecutionFailedResult arg1, ICommandContext arg2, IServiceProvider arg3)
        {
            return Task.CompletedTask;
        }

        public async Task<IUserMessage> SendMessageAsync(IEspeonContext context, string message, bool isTTS = false, Embed embed = null)
        {


            return null;
        }

        private Task<IMessage> GetOrDownloadMessageAsync(ulong channelId, ulong messageId)
        {
            if(!(_client.GetChannel(channelId) is SocketTextChannel channel))
                return null;

            return !(channel.GetCachedMessage(messageId) is IMessage message)
                ? channel.GetMessageAsync(messageId)
                : Task.FromResult(message);
        }
    }
}
