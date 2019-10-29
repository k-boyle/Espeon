

using Disqord;
using Disqord.Rest;
using Espeon.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IMessageService {
		Task<RestUserMessage> SendAsync(CachedUserMessage message, Action<NewMessageProperties> properties);
		Task DeleteMessagesAsync(CachedTextChannel channel, CachedMember bot, ulong userId, int amount);
	}
}