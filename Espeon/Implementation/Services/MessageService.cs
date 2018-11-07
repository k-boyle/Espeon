using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Espeon.Implementation.Entities;

namespace Espeon.Implementation.Services
{
    [Service(typeof(IMessageService), true)]
    public class MessageService : IMessageService
    {
        [Inject] private readonly ITimerService _timer;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IDatabaseService _database;

        private readonly IFixedQueue<Message> _queue;
        private const int CacheSize = 20;

        public MessageService()
        {
            _queue = new FixedQueue<Message>(CacheSize);
        }

        public async Task HandleReceivedMessageAsync(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message) ||
                message.Author.IsBot && message.Author.Id != _client.CurrentUser.Id) return;


        }

        public async Task<IUserMessage> SendMessageAsync(IEspeonContext context, string message, bool isTTS = false, Embed embed = null)
        {
            return null;
        }
    }
}
