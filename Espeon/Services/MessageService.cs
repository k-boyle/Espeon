using Casino.Common;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class MessageService : BaseService
    {
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly EmotesService _emotes;
        [Inject] private readonly LogService _logger;
        [Inject] private readonly Random _random;
        [Inject] private readonly TaskQueue _scheduler;
        [Inject] private readonly IServiceProvider _services;

        private readonly
            ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<Guid, CachedMessage>>>
            _messageCache;

        private static TimeSpan MessageLifeTime => TimeSpan.FromMinutes(10);

        public MessageService(IServiceProvider services) : base(services)
        {
            _messageCache =
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<Guid,
                    CachedMessage>>>();

            _commands.CommandErrored += args => CommandErroredAsync(new CasinoCommandErroredEventArgs
            {
                Context = args.Context as EspeonContext,
                Result = args.Result
            });
            _commands.CommandExecuted += CommandExecutedAsync;

            _client.MessageReceived += msg =>
                msg is SocketUserMessage message 
                    ? HandleReceivedMessageAsync(message, false) 
                    : Task.CompletedTask;

            _client.MessageReceived += msg =>
            {
                if (_random.NextDouble() >= 0.05)
                    return Task.CompletedTask;

                Task.Run(async () =>
                {
                    using var userStore = _services.GetService<UserStore>();

                    var user = await userStore.GetOrCreateUserAsync(msg.Author);
                    user.CandyAmount += 10;

                    await userStore.SaveChangesAsync();
                });

                return Task.CompletedTask;
            };

            _client.MessageUpdated += (_, msg, __) =>
                msg is SocketUserMessage message ? HandleReceivedMessageAsync(message, true) : Task.CompletedTask;
        }

        private async Task HandleReceivedMessageAsync(SocketUserMessage message, bool isEdit)
        {
            if (message.Author.IsBot && message.Author.Id != _client.CurrentUser.Id ||
                !(message.Channel is SocketTextChannel textChannel)) return;

            IReadOnlyCollection<string> prefixes;

            using (var guildStore = _services.GetService<GuildStore>())
            {
                var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild);

                prefixes = guild.Prefixes;

                if (guild.RestrictedChannels.Contains(textChannel.Id) || guild.RestrictedUsers.Contains(message.Author.Id))
                    return;
            }

            if (CommandUtilities.HasAnyPrefix(message.Content, prefixes, StringComparison.CurrentCulture,
                        out var prefix, out var output) || message.HasMentionPrefix(_client.CurrentUser, out output))
            {                
                var foundCommands = output.FindCommands();

                foreach (var command in foundCommands)
                {
                    var commandContext = new EspeonContext(_client, message, isEdit, prefix); //new context to handle db disposes

                    var result = await _commands.ExecuteAsync(command, commandContext, _services);

                    if (result is CommandNotFoundResult)
                    {
                        await message.AddReactionAsync(_emotes.Collection["BlobCat"]);
                    }
                    else if (!result.IsSuccessful && !(result is ExecutionFailedResult))
                    {
                        await CommandErroredAsync(new CasinoCommandErroredEventArgs
                        {
                            Context = commandContext,
                            Result = result as FailedResult
                        });
                    }
                }
            }
        }

        private async Task CommandErroredAsync(CasinoCommandErroredEventArgs args)
        {
            var context = args.Context;

            if (args.Result is ExecutionFailedResult failed)
            {
                await _logger.LogAsync(Source.Commands, Severity.Error, string.Empty, failed.Exception);

#if !DEBUG
                var c = _client.GetChannel(463299724326469634) as SocketTextChannel;

                await c.SendMessageAsync(failed.Exception.ToString().Substring(0, 500));
#endif
            }

            await SendAsync(context, x => x.Embed = Utilities.BuildErrorEmbed(args.Result, context));
        }

        private async Task CommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var context = args.Context as EspeonContext;

            await _logger.LogAsync(Source.Commands, Severity.Verbose,
                $"Successfully executed {{{context.Command.Name}}} for {{{context.User.GetDisplayName()}}} in {{{context.Guild.Name}/{context.Channel.Name}}}");

            context.Dispose();
        }

        /*
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
                CreatedAt = sentMessage.CreatedAt.ToUnixTimeMilliseconds()
            };

            var key = await _timer.EnqueueAsync(message, DateTimeOffset.UtcNow.Add(MessageLifeTime).ToUnixTimeMilliseconds(), RemoveAsync);

            _messageCache[context.Channel.Id][context.User.Id][key] = message;

            return sentMessage;
        }
        */

        public async Task<IUserMessage> SendAsync(EspeonContext context, Action<MessageProperties> properties)
        {
            if (!_messageCache.TryGetValue(context.Channel.Id, out var foundChannel))
                foundChannel = (_messageCache[context.Channel.Id] =
                    new ConcurrentDictionary<ulong, ConcurrentDictionary<Guid, CachedMessage>>());

            if (!foundChannel.TryGetValue(context.User.Id, out var foundCache))
                foundCache = (foundChannel[context.User.Id] = new ConcurrentDictionary<Guid, CachedMessage>());

            var messageProperties = properties.Invoke();

            var (guid, value) = foundCache.FirstOrDefault(x => x.Value.ExecutingId == context.Message.Id);

            IUserMessage sentMessage;
            if(value is null)
            {
                sentMessage = await SendMessageAsync(context, messageProperties);

                var message = new CachedMessage
                {
                    ChannelId = context.Channel.Id,
                    UserId = context.User.Id,
                    ExecutingId = context.Message.Id,
                    ResponseIds = new List<ulong>
                    {
                        sentMessage.Id
                    },
                    CreatedAt = sentMessage.CreatedAt.ToUnixTimeMilliseconds()
                };

                var key = _scheduler.ScheduleTask(message, 
                    DateTimeOffset.UtcNow.Add(MessageLifeTime).ToUnixTimeMilliseconds(), 
                    RemoveAsync);

                _messageCache[context.Channel.Id][context.User.Id][key] = message;

                return sentMessage;
            }

            if (context.IsEdit)
            {
                context.IsEdit = false;

                var perms = context.Guild.CurrentUser.GetPermissions(context.Channel).ManageMessages;
                await DeleteMessagesAsync(context, perms, new[] { (guid, value) });

                sentMessage = await SendMessageAsync(context, messageProperties);

                var message = new CachedMessage
                {
                    ChannelId = context.Channel.Id,
                    UserId = context.User.Id,
                    ExecutingId = context.Message.Id,
                    CreatedAt = sentMessage.CreatedAt.ToUnixTimeMilliseconds(),
                    ResponseIds = new List<ulong>
                    {
                        sentMessage.Id
                    }
                };

                var key = _scheduler.ScheduleTask(message,
                    DateTimeOffset.UtcNow.Add(MessageLifeTime).ToUnixTimeMilliseconds(),
                    RemoveAsync);

                _messageCache[context.Channel.Id][context.User.Id][key] = message;

                return sentMessage;
            }

            sentMessage = await SendMessageAsync(context, messageProperties);
            value.ResponseIds.Add(sentMessage.Id);

            return sentMessage;
        }

        //async needed for the cast
        private static async Task<IUserMessage> SendMessageAsync(EspeonContext context, MessageProperties properties)
        {
            if (properties.Stream is null)
            {
                return await context.Channel
                    .SendMessageAsync(properties.Content, embed: properties.Embed);
            }

            return await context.Channel.SendFileAsync(
                stream:     properties.Stream,
                filename:   properties.FileName,
                text:       properties.Content,
                embed:      properties.Embed);
        }

        private Task<IMessage> GetOrDownloadMessageAsync(ulong channelId, ulong messageId)
        {
            if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                return null;

            return !(channel.GetCachedMessage(messageId) is IMessage message)
                ? channel.GetMessageAsync(messageId)
                : Task.FromResult(message);
        }

        private Task RemoveAsync(Guid key, object removable)
        {
            var message = (CachedMessage)removable;
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

                var ordered = found.OrderByDescending(x => x.Value.CreatedAt).ToArray();
                amount = amount > ordered.Length ? ordered.Length : amount;

                var toDelete = new List<(Guid, CachedMessage)>();

                for (var i = 0; i < amount; i++)
                    toDelete.Add((ordered[i].Key, ordered[i].Value));

                var res = await DeleteMessagesAsync(context, manageMessages, toDelete);
                deleted += res;

            } while (deleted < amount);
        }

        private async Task<int> DeleteMessagesAsync(EspeonContext context, bool manageMessages,
            IEnumerable<(Guid Key, CachedMessage Cached)> messages)
        {
            var fetchedMessages = new List<IMessage>();

            foreach (var (key, cached) in messages)
            {
                await RemoveAsync(key, cached);
                _scheduler.CancelTask(key);

                foreach (var id in cached.ResponseIds)
                    fetchedMessages.Add(await GetOrDownloadMessageAsync(cached.ChannelId, id));
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

        private class CachedMessage
        {
            public ulong ChannelId { get; set; }
            //public ulong ResponseId { get; set; }
            public IList<ulong> ResponseIds { get; set; }
            public ulong ExecutingId { get; set; }
            public ulong UserId { get; set; }
            public long CreatedAt { get; set; }
        }

        public class MessageProperties
        {
            public string Content { get; set; }
            public Embed Embed { get; set; }
            public Stream Stream { get; set; }
            public string FileName { get; set; }
        }

        private class CasinoCommandErroredEventArgs
        {
            public EspeonContext Context { get; set; }
            public FailedResult Result { get; set; }
        }
    }
}
