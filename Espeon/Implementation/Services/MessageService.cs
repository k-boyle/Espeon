using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Espeon.Implementation.Entities;

namespace Espeon.Implementation.Services
{
    [Service(typeof(IMessageService))]
    public class MessageService : IMessageService
    {
        [Inject] private readonly ITimerService _timer;

        private readonly IFixedQueue<Message> _queue;
        private const int CacheSize = 20;

        public MessageService()
        {
            _queue = new FixedQueue<Message>(CacheSize);
        }

        public async Task HandleReceivedMessageAsync(SocketMessage message)
        {
        }

        public async Task<IUserMessage> SendMessageAsync(IEspeonContext context, string message, bool isTTS = false, Embed embed = null)
        {
            return null;
        }
    }
}
