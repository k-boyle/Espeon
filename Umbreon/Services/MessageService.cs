using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Interactive.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Core.Models;

namespace Umbreon.Services
{
    public class MessageService
    {
        private readonly InteractiveService _interactive;
        private readonly List<MessageModel> _messages = new List<MessageModel>();
        private ulong _currentMessage;

        public MessageService(InteractiveService interactive)
        {
            _interactive = interactive;
        }

        public async Task<IMessage> SendMessageAsync(ICommandContext context, string message, Embed embed = null, IPaginatedMessage paginator = null)
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
                    return await _interactive.SendPaginatedMessageAsync(context, paginator);
                }

                return retrievedMessage;
            }

            var sentMessage = paginator is null ? await context.Channel.SendMessageAsync(message, embed: embed) : await _interactive.SendPaginatedMessageAsync(context, paginator);
            var newMessage = new MessageModel(_currentMessage, context.User.Id, context.Channel.Id, sentMessage.Id, sentMessage.CreatedAt);
            _messages.Add(newMessage);
            return sentMessage;
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
