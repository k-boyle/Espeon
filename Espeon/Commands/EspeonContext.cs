using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Qmmands;

namespace Espeon.Commands
{
    public class EspeonContext : ICommandContext
    {
        private UserStore _userStore;
        public UserStore UserStore => _userStore ?? (_userStore = new UserStore());

        private GuildStore _guildStore;
        public GuildStore GuildStore => _guildStore ?? (_guildStore = new GuildStore());

        private CommandStore _commandStore;
        public CommandStore CommandStore => _commandStore ?? (_commandStore = new CommandStore());

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
