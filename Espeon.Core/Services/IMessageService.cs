using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface IMessageService
    {
        Task HandleReceivedMessageAsync(SocketMessage message);
        Task<IUserMessage> SendMessageAsync(IEspeonContext context, string message, Embed embed);
        Task DeleteMessagesAsync(IEspeonContext context, int amount);
    }
}
