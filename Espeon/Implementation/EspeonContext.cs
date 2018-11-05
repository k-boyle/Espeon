using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;

namespace Espeon.Implementation
{
    public class EspeonContext : IEspeonContext
    {
        public DiscordSocketClient Client { get; }
        public IUserMessage Message { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild => User.Guild;
        public SocketTextChannel Channel { get; }

        public EspeonContext(DiscordSocketClient client, IUserMessage message)
        {
            Client = client;
            Message = message;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;
        }
    }
}
