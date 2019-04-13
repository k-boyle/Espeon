using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public abstract class BaseService
    {
        protected BaseService(IServiceProvider services)
        {
            services.Inject(this);
        }

        public virtual Task InitialiseAsync(InitialiseArgs args)
            => Task.CompletedTask;
    }

    public class InitialiseArgs
    {
        public UserStore UserStore { get; set; }
        public GuildStore GuildStore { get; set; }
        public CommandStore CommandStore { get; set; }
        public IServiceProvider Services { get; set; }
    }
}
