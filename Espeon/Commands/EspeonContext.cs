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

        public GuildStore GuildStore { get; }

        private CommandStore _commandStore;
        public CommandStore CommandStore => _commandStore ?? (_commandStore = new CommandStore());

        public DiscordSocketClient Client { get; }
        public IUserMessage Message { get; }
        public SocketGuildUser User { get; }
        public SocketGuild Guild => User.Guild;
        public SocketTextChannel Channel { get; }

        public bool IsEdit { get; set; }
        public string PrefixUsed { get; }

        public User Invoker { get; private set; }
        public Guild CurrentGuild { get; }

        private EspeonContext(GuildStore guildStore, Guild currentGuild, DiscordSocketClient client,
            IUserMessage message, bool isEdit, string prefix)
        {
            GuildStore = guildStore;
            Client = client;
            Message = message;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;

            IsEdit = isEdit;
            PrefixUsed = prefix;
            CurrentGuild = currentGuild;
        }

        public static async Task<EspeonContext> CreateAsync(GuildStore guildStore, Guild currentGuild,
            DiscordSocketClient client, IUserMessage message, bool isEdit, string prefix)
        {
            var ctx = new EspeonContext(guildStore, currentGuild, client, message, isEdit, prefix);
            ctx.Invoker = await ctx.GetInvokerAsync();

            return ctx;
        }

        private Task<User> GetInvokerAsync()
            => UserStore.GetOrCreateUserAsync(User);

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    GuildStore.Dispose();
                    _userStore?.Dispose();
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
