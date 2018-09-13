using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.Contexts;
using Umbreon.Core.Entities;
using Umbreon.Core.Entities.Guild;
using Umbreon.Extensions;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Paginator;
using Umbreon.Interfaces;
using Umbreon.Paginators;
using Umbreon.Paginators.CommandMenu;

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

        private const int CacheSize = 20;

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
                var command = message.Content.Substring(argPos).RemoveExtraSpaces();
                await HandleCommandAsync(context, command);
            }
        }

        public Task HandleMessageUpdateAsync(SocketMessage msg)
            => HandleMessageAsync(msg);

        private async Task HandleCommandAsync(UmbreonContext context, string command)
        {
            if (!context.Guild.CurrentUser.GetPermissions(context.Channel).SendMessages) return;
            var result = await _commands.ExecuteAsync(context, command, _services);
            if (!result.Result.IsSuccess)
                await HandleErrorAsync(context, result);
        }

        private async Task HandleErrorAsync(ICommandContext context, (IResult Result, CommandInfo Command) result)
        {
            var usage = result.Command.Attributes.OfType<UsageAttribute>().FirstOrDefault();

            switch (result.Result.Error)
            {
                case CommandError.ParseFailed:
                    await NewMessageAsync(context, $"I failed to parse your argument, this command is meant to be used like; `{usage?.Example}`");
                    break;

                case CommandError.BadArgCount:
                    await NewMessageAsync(context, $"You didn't give me enough arguments, this command is meant to be used like; `{usage?.Example}`");
                    break;

                case CommandError.UnmetPrecondition:
                    await NewMessageAsync(context, $"Uhoh, you don't have the right permissions to use this command, you need; {result.Result.ErrorReason}");
                    break;

                case CommandError.Exception:
                    if (result.Result.ErrorReason.Contains("502"))
                    {
                        await NewMessageAsync(context, "Discord did a goof, try again");
                        break;
                    }

                    await NewMessageAsync(context, "Something unexpected happened... The error has been forwarded to the proper authorities");
                    if (_client.GetChannel(463299724326469634) is SocketTextChannel channel)
                    {
                        await channel.SendMessageAsync($"{context.Message} - {result.Result.ErrorReason}");
                    }
                    break;

                case CommandError.Unsuccessful:
                    await NewMessageAsync(context, result.Result.ErrorReason);
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
            await NewItemAsync(userId, channelId, response.CreatedAt, executingId, response.Id, false);

            return response;
        }

        public Task<IUserMessage> SendFileAsync(ICommandContext context, Stream stream, string content = null,
            bool isTTS = false,
            Embed embed = null)
            => SendFileAsync(context.User.Id, context.Message.Id, context.Channel.Id, stream, content, isTTS, embed);

        public async Task<IUserMessage> SendFileAsync(ulong userId, ulong executingId, ulong channelId, Stream stream, string content = null, bool isTTS = false,
            Embed embed = null)
        {
            if (!(_client.GetChannel(channelId) is SocketTextChannel channel)) return null;
            var response = await channel.SendFileAsync(stream, "image.png", content, isTTS, embed);
            await NewItemAsync(userId, channelId, response.CreatedAt, executingId, response.Id, true);
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
                case CommandMenuMessage cmd:
                    callback = new CommandMenuCallback(_commands, _services, _interactive, cmd, this, context);
                    break;

                case PaginatedMessage pager:
                    callback = new PaginatedMessageCallback(_interactive, context, pager);
                    break;
            }

            if (callback == null) return null;
            await callback.DisplayAsync().ConfigureAwait(false);

            await NewItemAsync(context.User.Id, context.Channel.Id, callback.Message.CreatedAt, context.Message.Id,
                callback.Message.Id, false);

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

        private Task NewItemAsync(ulong userId, ulong channelId, DateTimeOffset createdAt, ulong executingId, ulong responseId, bool attachedFile)
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
                AttachedFile = attachedFile,

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
            if (found.AttachedFile) return null;
            var retrievedMessage = await GetOrDownloadMessageAsync(context, found.ResponseId);
            return retrievedMessage as IUserMessage;
        }

        private static Task<IMessage> GetOrDownloadMessageAsync(ICommandContext context, ulong messageId)
            => context.Channel.GetMessageAsync(messageId, CacheMode.CacheOnly) ??
               context.Channel.GetMessageAsync(messageId);
    }
}
