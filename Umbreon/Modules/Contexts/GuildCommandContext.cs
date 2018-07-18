using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Umbreon.Modules.Contexts
{
    public class GuildCommandContext : ICommandContext
    {
        public DiscordSocketClient Client { get; }
        public SocketGuild Guild { get; }
        public SocketGuildChannel GuildChannel { get; }
        public SocketTextChannel Channel { get; }
        public SocketGuildUser User { get; }
        public SocketUserMessage Message { get; }

        public GuildCommandContext(DiscordSocketClient client, SocketUserMessage msg)
        {
            Client = client;
            Guild = (msg.Channel as SocketGuildChannel)?.Guild;
            GuildChannel = msg.Channel as SocketGuildChannel;
            Channel = msg.Channel as SocketTextChannel;
            User = msg.Author as SocketGuildUser;
            Message = msg;
        }

        IDiscordClient ICommandContext.Client => Client;
        IGuild ICommandContext.Guild => Guild;
        IMessageChannel ICommandContext.Channel => Channel;
        IUser ICommandContext.User => User;
        IUserMessage ICommandContext.Message => Message;
    }
}
