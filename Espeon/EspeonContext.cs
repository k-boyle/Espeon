using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;

namespace Espeon
{
    public class EspeonContext : IEspeonContext
    {
        public DiscordSocketClient Client { get; }
        public IUserMessage Message { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild => User.Guild;
        public SocketTextChannel Channel { get; }

        public bool IsEdit { get; }

        public EspeonContext(DiscordSocketClient client, IUserMessage message, bool isEdit)
        {
            Client = client;
            Message = message;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;

            IsEdit = isEdit;
        }
    }
}
