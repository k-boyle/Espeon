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
        public string PrefixUsed { get; }

        public User Invoker { get; private set; }
        public Guild CurrentGuild { get; private set; }

        private EspeonContext(DiscordSocketClient client, IUserMessage message, bool isEdit, string prefix)
        {
            Client = client;
            Message = message;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;

            IsEdit = isEdit;
            PrefixUsed = prefix;
        }

        public static async Task<EspeonContext> CreateAsync(DiscordSocketClient client, IUserMessage message,
            bool isEdit, string prefix)
        {
            var ctx = new EspeonContext(client, message, isEdit, prefix);
            ctx.Invoker = await ctx.UserStore.GetOrCreateUserAsync(ctx.User);
            ctx.CurrentGuild = await ctx.GuildStore.GetOrCreateGuildAsync(ctx.Guild);

            return ctx;
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _guildStore?.Dispose();
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
