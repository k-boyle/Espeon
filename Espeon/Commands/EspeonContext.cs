using Discord;
using Discord.WebSocket;
using Espeon.Database;
using Qmmands;

namespace Espeon.Commands
{
    public class EspeonContext : ICommandContext
    {
        public DatabaseContext Database { get; set; }

        public DiscordSocketClient Client { get; }
        public IUserMessage Message { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild => User.Guild;
        public SocketTextChannel Channel { get; }

        public bool IsEdit { get; }

        public EspeonContext(DatabaseContext database, DiscordSocketClient client, IUserMessage message, bool isEdit)
        {
            Database = database;

            Client = client;
            Message = message;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;

            IsEdit = isEdit;
        }
    }
}
