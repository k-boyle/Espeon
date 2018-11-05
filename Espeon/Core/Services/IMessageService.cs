using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;

namespace Espeon.Core.Services
{
    public interface IMessageService
    {
        Task HandleReceivedMessageAsync(SocketMessage message);
        Task<IUserMessage> SendMessageAsync(IEspeonContext context, string message, bool isTTS, Embed embed);
    }
}
