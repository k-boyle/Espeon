using Casino.DependencyInjection;
using Espeon.Services;
using System;

namespace Espeon.Bot.Services
{
    public class DelayedCommandService : BaseService<InitialiseArgs>, IDelayedCommandService
    {
        public DelayedCommandService(IServiceProvider services) : base(services)
        {
        }
    }
}
