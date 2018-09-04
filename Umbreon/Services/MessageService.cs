using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Attributes;
using Umbreon.Commands.Contexts;
using Umbreon.Core.Entities;
using Umbreon.Core.Entities.Guild;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Paginator;
using Umbreon.Interfaces;
using Umbreon.Paginators;
using Umbreon.Paginators.CommandMenu;
using Umbreon.Paginators.HelpPaginator;

namespace Umbreon.Services
{
    [Service]
    public class MessageService : IRemoveableService
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseService _database;
        private readonly CommandService _commands;
        private readonly InteractiveService _interactive;
        private readonly TimerService _timer;
        private readonly CandyService _candy;
        private readonly IServiceProvider _services;
        private readonly Random _random;

        private const int CacheSize = 10;

        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<Message>> _messageCache =
            new ConcurrentDictionary<ulong, ConcurrentQueue<Message>>();

        public MessageService(DiscordSocketClient client, DatabaseService database, CommandService commands, InteractiveService interactive, TimerService timer, CandyService candy, IServiceProvider services, Random random)
        {
            _client = client;
            _database = database;
            _commands = commands;
            _interactive = interactive;
            _timer = timer;
            _candy = candy;
            _services = services;
            _random = random;
        }

        public Task RemoveAsync(IRemoveable obj)
        {
            if (!(obj is Message message)) return Task.CompletedTask;
            if (!_messageCache.TryGetValue(message.UserId, out var found)) return Task.CompletedTask;
            var newQueue = new ConcurrentQueue<Message>();
            foreach (var item in found)
            {
                if (item.ResponseId == message.ResponseId) continue;
                newQueue.Enqueue(item);
            }

            if (newQueue.IsEmpty)
                _messageCache.TryRemove(message.UserId, out _);
            else
                _messageCache[message.UserId] = newQueue;
            return Task.CompletedTask;
        }

        public async Task HandleMessageAsync(SocketMessage msg)
        {
            if (msg.Author.IsBot || string.IsNullOrEmpty(msg.Content) || !(msg.Channel is SocketGuildChannel channel) ||
                !(msg is SocketUserMessage message)) return;

            if (_random.Next(100) < 10)
            {
                _candy.UpdateCandies(message.Author.Id, false, 1);
            }

            var guild = _database.TempLoad<GuildObject>("guilds", channel.Guild.Id);

            if (guild.BlacklistedUsers.Contains(message.Author.Id) ||
                guild.RestrictedChannels.Contains(channel.Id) ||
                guild.UseWhiteList && !guild.WhiteListedUsers.Contains(message.Author.Id)) return;

            var prefixes = guild.Prefixes;

            var argPos = 0;

            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                prefixes.Any(x => message.HasStringPrefix(x, ref argPos)))
            {
                guild.When = DateTime.UtcNow + TimeSpan.FromDays(1);
                _timer.Update(guild);
                _database.UpdateObject("guilds", guild);

                var context = new UmbreonContext(_client, message, _services.GetService<HttpClient>());
                await HandleCommandAsync(context, argPos);
            }
        }

        public Task HandleMessageUpdateAsync(SocketMessage msg)
            => HandleMessageAsync(msg);

        private async Task HandleCommandAsync(UmbreonContext context, int argPos)
        {
            if (!context.Guild.CurrentUser.GetPermissions(context.Channel).SendMessages) return;
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(CommandInfo command, ICommandContext context, IResult result)
        {
            if (result.IsSuccess) return;
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    if (guild.UnkownCommandResult)
                    {
                        var prefixes = guild.Prefixes;
                        var argPos = 0;
                        if (context.Message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                            prefixes.Any(x => context.Message.HasStringPrefix(x, ref argPos))) ;

                        var i = 1;
                        var commands = _commands.Search(context, argPos).Commands.Select(x => $"{i++}{x.Command.Aliases.First()}").ToArray();
                        await SendMessageAsync(context, "Command not found, did you mean:\n" +
                                                        $"{string.Join("\n", commands, 0, 3)}");
                    }

                    break;

                case CommandError.ParseFailed:
                    await SendMessageAsync(context, "Failed to parse command");
                    break;

                case CommandError.BadArgCount:
                    var usage = command.Attributes.OfType<UsageAttribute>().Single().Example;
                    await SendMessageAsync(context, $"Wrong command usage, here have an example:\n{usage}");
                    break;

                case CommandError.UnmetPrecondition:
                    await SendMessageAsync(context, result.ErrorReason);
                    break;

                case CommandError.Exception:
                    await SendMessageAsync(context,
                        "There was an unexpected result... The error has been reported, try again, if this persists please wait until a fix is released");
                    await NewMessageAsync(0, 0, 463299724326469634, result.ErrorReason);
                    break;
            }
        }

        public async Task<IUserMessage> SendMessageAsync(ICommandContext context, string content, bool isTTS = false,
            Embed embed = null)
        {
            var message = await GetExistingMessageAsync(context);
            if (message is null)
            {
                return await NewMessageAsync(context, content, isTTS, embed);
            }

            var currentUser = await context.Guild.GetCurrentUserAsync();
            var perms = currentUser.GetPermissions(context.Channel as IGuildChannel);

            if (perms.ManageMessages)
                await message.RemoveAllReactionsAsync();

            await message.ModifyAsync(x =>
            {
                x.Content = content;
                x.Embed = embed;
            });

            return message;
        }

        public Task<IUserMessage> NewMessageAsync(ICommandContext context, string content, bool isTTS = false,
            Embed embed = null)
            => NewMessageAsync(context.User.Id, context.Message.Id, context.Channel.Id, content, isTTS, embed);

        public async Task<IUserMessage> NewMessageAsync(ulong userId, ulong executingId, ulong channelId, string content,
            bool isTTS = false, Embed embed = null)
        {
            if (!(_client.GetChannel(channelId) is SocketTextChannel channel)) return null;
            var response = await channel.SendMessageAsync(content, isTTS, embed);
            await NewItem(userId, channelId, response.CreatedAt, executingId, response.Id);

            return response;
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(ICommandContext context, BasePaginator paginator)
        {
            var message = await GetExistingMessageAsync(context);

            if (!(message is null))
            {
                await message.DeleteAsync();
            }

            ICallback callback = null;

            switch (paginator)
            {
                case HelpPaginatedMessage help:
                    callback = new HelpPaginatedCallback(_interactive, context, help);
                    break;

                case CommandMenuMessage cmd:
                    callback = new CommandMenuCallback(_commands, _services, _interactive, cmd, this, context);
                    break;

                case PaginatedMessage pager:
                    callback = new PaginatedMessageCallback(_interactive, context, pager);
                    break;
            }

            if (callback == null) return null;
            await callback.DisplayAsync().ConfigureAwait(false);

            await NewItem(context.User.Id, context.Channel.Id, callback.Message.CreatedAt, context.Message.Id,
                callback.Message.Id);

            return callback.Message;

        }

        public async Task<int> ClearMessagesAsync(ICommandContext context, int amount)
        {
            if (!_messageCache.TryGetValue(context.User.Id, out var found)) return 0;
            amount = amount > found.Count ? found.Count + 1 : amount;
            var matching = found.Where(x => x.ChannelId == context.Channel.Id).TakeWhile(item => amount-- != 0);
            var retrieved = new List<IMessage>();
            foreach (var item in matching)
            {
                var msg = await GetOrDownloadMessageAsync(context, item.ResponseId);
                if (msg is null) continue;
                retrieved.Add(msg);
            }

            var currentUser = await context.Guild.GetCurrentUserAsync();
            var perms = currentUser.GetPermissions(context.Channel as IGuildChannel);

            if (perms.ManageMessages)
            {
                if (context.Channel is ITextChannel channel)
                {
                    await channel.DeleteMessagesAsync(retrieved);
                }
            }
            else
            {
                foreach (var message in retrieved)
                    await context.Channel.DeleteMessageAsync(message);
            }

            var newQueue = new ConcurrentQueue<Message>();
            var delIds = retrieved.Select(x => x.Id);
            foreach (var item in found)
            {
                if (delIds.Contains(item.ResponseId)) continue;
                newQueue.Enqueue(item);
            }

            _timer.Remove(newQueue);

            if (newQueue.IsEmpty)
                _messageCache.TryRemove(context.User.Id, out _);
            else
                _messageCache[context.User.Id] = newQueue;

            return retrieved.Count(x => !(x is null));
        }

        public async Task DeleteMessageAsync(ICommandContext context, IUserMessage message)
        {
            if (_messageCache.TryGetValue(context.User.Id, out var found))
            {
                if (found.Any(x => x.ResponseId == message.Id))
                {
                    await message.DeleteAsync();
                    _messageCache[context.User.Id] =
                        new ConcurrentQueue<Message>(found.Where(x => x.ResponseId != message.Id));
                    if (_messageCache.TryGetValue(context.User.Id, out var newFound))
                    {
                        if (newFound.IsEmpty)
                            _messageCache.TryRemove(context.User.Id, out _);
                    }
                }
            }
        }

        private Task NewItem(ulong userId, ulong channelId, DateTimeOffset createdAt, ulong executingId, ulong responseId)
        {
            _messageCache.TryAdd(userId, new ConcurrentQueue<Message>());
            if (!_messageCache.TryGetValue(userId, out var found)) return null;
            if (found.Count >= CacheSize)
                found.TryDequeue(out _);

            var newMessage = new Message
            {
                UserId = userId,
                ChannelId = channelId,
                CreatedAt = createdAt,
                ExecutingId = executingId,
                ResponseId = responseId,

                Service = this,
                Identifier = _random.Next()
            };

            found.Enqueue(newMessage);

            _timer.Enqueue(newMessage);
            _messageCache[userId] = found;
            return Task.CompletedTask;
        }

        private async Task<IUserMessage> GetExistingMessageAsync(ICommandContext context)
        {
            if (!_messageCache.TryGetValue(context.User.Id, out var queue)) return null;
            var found = queue.FirstOrDefault(x => x.ExecutingId == context.Message.Id);
            if (found is null) return null;
            var retrievedMessage = await GetOrDownloadMessageAsync(context, found.ResponseId);
            return retrievedMessage as IUserMessage;
        }

        private static Task<IMessage> GetOrDownloadMessageAsync(ICommandContext context, ulong messageId)
            => context.Channel.GetMessageAsync(messageId, CacheMode.CacheOnly) ??
               context.Channel.GetMessageAsync(messageId);
    }
}
