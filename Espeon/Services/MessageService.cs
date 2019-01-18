using Discord;
using Discord.WebSocket;
using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Database;
using Espeon.Database.Entities;
using Espeon.Entities;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class MessageService : IService
    {
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly LogService _logger;
        [Inject] private readonly IServiceProvider _services;
        [Inject] private readonly TimerService _timer;

        private readonly
            ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<string, CachedMessage>>>
            _messageCache;

        private static TimeSpan MessageLifeTime => TimeSpan.FromMinutes(10);

        public MessageService()
        {
            _messageCache =
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<string,
                    CachedMessage>>>();
        }

        public Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
        {
            var commands = services.GetService<CommandService>();
            var client = services.GetService<DiscordSocketClient>();

            commands.CommandErrored += CommandErroredAsync;
            commands.CommandExecuted += CommandExecutedAsync;

            client.MessageReceived += msg =>
                msg is SocketUserMessage message ? HandleReceivedMessageAsync(message, false) : Task.CompletedTask;
            client.MessageUpdated += (_, msg, __) =>
                msg is SocketUserMessage message ? HandleReceivedMessageAsync(message, true) : Task.CompletedTask;

            return Task.CompletedTask;
        }
        
        private async Task HandleReceivedMessageAsync(SocketUserMessage message, bool isEdit)
        {
            if (message.Author.IsBot && message.Author.Id != _client.CurrentUser.Id ||
                message.Channel is IPrivateChannel) return;

            using (var scope = _services.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();

                var commandContext = new EspeonContext(databaseContext, _client, message, isEdit);
                var guild = await databaseContext.Guilds.FindAsync(commandContext.Guild.Id);

                if (guild is null)
                {
                    guild = new Guild
                    {
                        Id = commandContext.Guild.Id
                    };

                    await databaseContext.Guilds.AddAsync(guild);
                }

                var prefixes = guild.Config.Prefixes;

                if (CommandUtilities.HasAnyPrefix(message.Content, prefixes, StringComparison.CurrentCulture,
                        out _, out var output) || message.HasMentionPrefix(_client.CurrentUser, out output))
                {
                    var foundCommands = output.FindCommands();

                    foreach(var command in foundCommands)
                    {
                        var result = await _commands.ExecuteAsync(command, commandContext, _services);
                        await commandContext.Database.SaveChangesAsync();

                        if (!result.IsSuccessful && !(result is ExecutionFailedResult))
                            await CommandErroredAsync(result as FailedResult, commandContext, _services);
                    }
                }
            }
        }

        private async Task CommandErroredAsync(FailedResult result, ICommandContext originalContext, IServiceProvider services)
        {
            if (!(originalContext is EspeonContext context))
                return;

            Embed response = null;

            switch (result)
            {
                case ArgumentParseFailedResult argumentParseFailedResult:
                    response = argumentParseFailedResult.GenerateResponse(context);
                    break;

                case ChecksFailedResult checksFailedResult:
                    response = checksFailedResult.GenerateResponse(context);
                    break;

                //TODO
                case CommandOnCooldownResult commandOnCooldownResult:
                    break;

                case ExecutionFailedResult executionFailedResult:
                    response = executionFailedResult.GenerateResponse(context);

                    await _logger.LogAsync(Source.Commands, Severity.Critical, executionFailedResult.Reason,
                        executionFailedResult.Exception);
                    break;

                case OverloadsFailedResult overloadsFailedResult:
                    response = overloadsFailedResult.GenerateResponse(context);
                    break;

                case ParameterChecksFailedResult parameterChecksFailedResult:
                    response = parameterChecksFailedResult.GenerateResponse(context);
                    break;

                case TypeParseFailedResult typeParseFailedResult:
                    response = typeParseFailedResult.GenerateResponse(context);
                    break;
            }

            await SendMessageAsync(context, string.Empty, response);
        }

        private async Task CommandExecutedAsync(Command command, CommandResult originalResult,
            ICommandContext originalContext, IServiceProvider services)
        {
            if (!(originalContext is EspeonContext context))
                return;

            await _logger.LogAsync(Source.Commands, Severity.Verbose,
                $"Successfully executed {{{command.Name}}} for {{{context.User.GetDisplayName()}}} in {{{context.Guild.Name}/{context.Channel.Name}}}");
        }

        public async Task<IUserMessage> SendMessageAsync(EspeonContext context, string content, Embed embed = null)
        {
            if (!_messageCache.TryGetValue(context.Channel.Id, out var foundChannel))
                foundChannel = (_messageCache[context.Channel.Id] =
                    new ConcurrentDictionary<ulong, ConcurrentDictionary<string, CachedMessage>>());

            if (!foundChannel.TryGetValue(context.User.Id, out var foundCache))
                foundCache = (foundChannel[context.User.Id] = new ConcurrentDictionary<string, CachedMessage>());

            var foundMessage = foundCache.FirstOrDefault(x => x.Value.ExecutingId == context.Message.Id);

            if (context.IsEdit && !foundMessage.Equals(default(KeyValuePair<string, CachedMessage>)))
            {
                if (await GetOrDownloadMessageAsync(foundMessage.Value.ChannelId, foundMessage.Value.ResponseId) is
                    IUserMessage fetchedMessage)
                {
                    await fetchedMessage.ModifyAsync(x =>
                    {
                        x.Content = content;
                        x.Embed = embed;
                    });

                    return fetchedMessage;
                }
            }

            var sentMessage = await context.Channel.SendMessageAsync(content, embed: embed);

            var message = new CachedMessage
            {
                ChannelId = context.Channel.Id,
                ExecutingId = context.Message.Id,
                UserId = context.User.Id,
                ResponseId = sentMessage.Id,
                WhenToRemove = DateTimeOffset.UtcNow.Add(MessageLifeTime).ToUnixTimeMilliseconds()
            };

            var key = await _timer.EnqueueAsync(message, RemoveAsync);

            _messageCache[context.Channel.Id][context.User.Id][key] = message;

            return sentMessage;
        }

        private Task<IMessage> GetOrDownloadMessageAsync(ulong channelId, ulong messageId)
        {
            if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                return null;

            return !(channel.GetCachedMessage(messageId) is IMessage message)
                ? channel.GetMessageAsync(messageId)
                : Task.FromResult(message);
        }

        private Task RemoveAsync(string key, IRemovable removable)
        {
            var message = (CachedMessage) removable;
            _messageCache[message.ChannelId][message.UserId].TryRemove(key, out _);

            if (_messageCache[message.ChannelId][message.UserId].Count == 0)
                _messageCache.Remove(message.UserId, out _);

            if (_messageCache[message.ChannelId].Count == 0)
                _messageCache.Remove(message.ChannelId, out _);

            return Task.CompletedTask;
        }

        public async Task DeleteMessagesAsync(EspeonContext context, int amount)
        {
            var perms = context.Guild.CurrentUser.GetPermissions(context.Channel);
            var manageMessages = perms.ManageMessages;

            var deleted = 0;

            do
            {
                if (!_messageCache.TryGetValue(context.Channel.Id, out var foundCache))
                    return;

                if (!foundCache.TryGetValue(context.User.Id, out var found))
                    return;

                if (found is null)
                    return;

                if (found.Count == 0)
                {
                    _messageCache[context.Channel.Id].Remove(context.User.Id, out _);

                    if (_messageCache[context.Channel.Id].Count == 0)
                        _messageCache.Remove(context.Channel.Id, out _);

                    return;
                }

                var ordered = found.OrderByDescending(x => x.Value.WhenToRemove).ToImmutableArray();
                amount = amount > ordered.Length ? ordered.Length : amount;

                var toDelete = new List<(string, CachedMessage)>();

                for (var i = 0; i < amount; i++)
                    toDelete.Add((ordered[i].Key, ordered[i].Value));

                var res = await DeleteMessagesAsync(context, manageMessages, toDelete);
                deleted += res;

            } while (deleted < amount);
        }

        private async Task<int> DeleteMessagesAsync(EspeonContext context, bool manageMessages,
            IEnumerable<(string Key, CachedMessage Cached)> messages)
        {
            var fetchedMessages = new List<IMessage>();

            foreach (var (key, cached) in messages)
            {
                await RemoveAsync(key, cached);
                await _timer.RemoveAsync(key);

                if (await GetOrDownloadMessageAsync(cached.ChannelId, cached.ResponseId) is IMessage fetchedMessage)
                    fetchedMessages.Add(fetchedMessage);
            }

            if (manageMessages)
            {
                await context.Channel.DeleteMessagesAsync(fetchedMessages);
            }
            else
            {
                foreach (var message in fetchedMessages)
                    await context.Channel.DeleteMessageAsync(message);
            }

            return fetchedMessages.Count;
        }
    }
}
