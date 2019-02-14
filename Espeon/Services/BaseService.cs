using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public abstract class BaseService
    {
        public virtual Task InitialiseAsync(UserStore userStore, GuildStore guildStore, CommandStore commandStore, IServiceProvider services)
            => Task.CompletedTask;
    }
}
