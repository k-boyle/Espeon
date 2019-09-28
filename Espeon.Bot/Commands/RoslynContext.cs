using Discord;
using Espeon.Commands;
using Espeon.Entities;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RoslynContext
    {
        public EspeonContext Context { get; }
        public IServiceProvider Services { get; }

        public RoslynContext(EspeonContext context, IServiceProvider services)
        {
            Context = context;
            Services = services;
        }

        public async Task<IUserMessage> SendAsync(Action<NewMessageProperties> func)
        {
            var message = Services.GetService<IMessageService>();

            return await message.SendAsync(Context, func);
        }
    }
}
