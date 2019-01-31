using Discord;
using Discord.WebSocket;
using Espeon.Database;
using Espeon.Database.Entities;
using Qmmands;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class EspeonContext : ICommandContext
    {
        private DatabaseContext _database;
        public DatabaseContext Database => _database ?? (_database = new DatabaseContext());

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

        public Task<Guild> GetCurrentGuildAsync()
            => Database.GetCurrentGuildAsync(this);

        public Task<Guild> GetCurrentGuildAsync<TProp>(Expression<Func<Guild, TProp>> expression)
            => Database.GetCurrentGuildAsync(this, expression);
    }
}
