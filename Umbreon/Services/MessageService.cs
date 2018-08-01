using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Interactive.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive.HelpPaginator;
using Umbreon.Controllers.CommandMenu;
using Umbreon.Core.Models;

namespace Umbreon.Services
{
    public class MessageService : InteractiveService
    {
        // TODO overhaul this garbage
        private readonly List<MessageModel> _messages = new List<MessageModel>();
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private ulong _currentMessage;

        public MessageService(DiscordSocketClient client, CommandService commands, IServiceProvider services) : base(client)
        {
            _commands = commands;
            _services = services;
        }

        public async Task<IUserMessage> SendMessageAsync(ICommandContext context, string message, Embed embed = null, IPaginatedMessage paginator = null)
        {
            CleanseOldMessages();
            if (_messages.Any(x => x.ExecutingMessageId == _currentMessage))
            {
                var targetMessage = _messages.FirstOrDefault(x => x.ExecutingMessageId == _currentMessage);
                var retrievedMessage = (context.Channel as SocketTextChannel).GetCachedMessage(targetMessage.MessageId) ??
                                       await context.Channel.GetMessageAsync(targetMessage.MessageId);
                if (retrievedMessage is null) return null;
                if (paginator is null)
                {
                    if ((await context.Guild.GetCurrentUserAsync()).GetPermissions(context.Channel as SocketGuildChannel)
                        .ManageMessages)
                        await (retrievedMessage as SocketUserMessage).RemoveAllReactionsAsync();
                    await (retrievedMessage as IUserMessage).ModifyAsync(x =>
                    {
                        x.Content = message;
                        x.Embed = embed;
                    });
                }
                else
                {
                    await retrievedMessage.DeleteAsync();
                    return await SendPaginatedMessageAsync(context, paginator);
                }

                return retrievedMessage as IUserMessage;
            }

            var sentMessage = paginator is null ? await context.Channel.SendMessageAsync(message, embed: embed) : await SendPaginatedMessageAsync(context, paginator);
            var newMessage = new MessageModel(_currentMessage, context.User.Id, context.Channel.Id, sentMessage.Id, sentMessage.CreatedAt);
            _messages.Add(newMessage);
            return sentMessage;
        }

        private new async Task<IUserMessage> SendPaginatedMessageAsync(ICommandContext context, IPaginatedMessage pager, ICriterion<SocketReaction> criterion = null)
        {
            ICallback callback;

            switch (pager)
            {
                case HelpPaginatedMessage helpPaginatedMessage:
                    callback = new HelpPaginatedCallback(this, context, helpPaginatedMessage);
                    break;
                case PaginatedMessage paginatedMessage:
                    callback = new PaginatedMessageCallback(this, context, paginatedMessage);
                    break;
                case CommandMenuProperties commandMenuProperties:
                    callback = new CommandMenu(this, context, commandMenuProperties, _commands, _services, Discord);
                    break;
                default:
                    callback = null;
                    break;
            }

            await callback.DisplayAsync().ConfigureAwait(false);

            return callback.Message;
        }

        public async Task ClearMessages(ICommandContext context)
        {
            CleanseOldMessages();
            var foundMessages = _messages.Where(x => x.UserId == context.User.Id && x.ChannelId == context.Channel.Id).ToList();
            foreach (var foundMessage in foundMessages)
            {
                _messages.Remove(foundMessage);
                var retrievedMessage = (context.Channel as SocketTextChannel).GetCachedMessage(foundMessage.MessageId) ??
                                       await context.Channel.GetMessageAsync(foundMessage.MessageId);
                if (retrievedMessage == null) continue;
                await retrievedMessage.DeleteAsync();
            }
        }

        private void CleanseOldMessages()
        {
            var oldMessages = _messages.Where(x => x.CreatedAt.AddMinutes(5) < DateTime.UtcNow).ToList();
            foreach (var oldMessage in oldMessages)
            {
                _messages.Remove(oldMessage);
            }
        }

        public void SetCurrentMessage(ulong receivedMessageId)
        {
            _currentMessage = receivedMessageId;
        }
    }
}
