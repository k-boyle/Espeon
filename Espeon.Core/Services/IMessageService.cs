using Discord.Rest;
using Espeon.Core.Commands;
using Espeon.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IMessageService {
		Task<RestUserMessage> SendAsync(EspeonContext context, Action<NewMessageProperties> properties);
		Task DeleteMessagesAsync(EspeonContext context, int amount);
	}
}