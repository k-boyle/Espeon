using Discord.Rest;
using Discord.WebSocket;
using Espeon.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IMessageService {
		Task<RestUserMessage> SendAsync(SocketUserMessage message, Action<NewMessageProperties> properties);
		Task DeleteMessagesAsync(SocketTextChannel channel, SocketGuildUser bot, ulong userId, int amount);
	}
}