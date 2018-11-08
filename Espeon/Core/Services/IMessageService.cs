using Discord.WebSocket;
using Espeon.Core.Commands;
using System.Threading.Tasks;
using Discord;

namespace Espeon.Core.Services
{
    public interface IMessageService<in T> where T : IEspeonContext
    {
        Task HandleReceivedMessageAsync(SocketMessage message);
        Task<IUserMessage> SendMessageAsync(T context, string message, Embed embed);
    }
}
