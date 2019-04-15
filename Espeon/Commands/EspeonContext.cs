using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Qmmands;
using System;
using System.Threading.Tasks;
using Espeon.Databases;

namespace Espeon.Commands
{
    public class EspeonContext : CommandContext, IDisposable
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

        public bool IsEdit { get; set; }
        public string PrefixUsed { get; set; }

        public EspeonContext(DiscordSocketClient client, IUserMessage message, bool isEdit, string prefix)
        {
            Client = client;
            Message = message;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;

            IsEdit = isEdit;
            PrefixUsed = prefix;
        }

        public Task<User> GetInvokerAsync()
            => UserStore.GetOrCreateUserAsync(User);

        public Task<Guild> GetCurrentGuildAsync()
            => GuildStore.GetOrCreateGuildAsync(Guild);

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _userStore?.Dispose();
                    _guildStore?.Dispose();
                    _commandStore?.Dispose();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
