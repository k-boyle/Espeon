using Discord;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using MessageProperties = Espeon.Services.MessageService.MessageProperties;

namespace Espeon.Commands
{
    public class RoslynContext
    {
        public EspeonContext Context { get; set; }
        public IServiceProvider Services { get; set; }

        public Task<IUserMessage> SendAsync(Action<MessageProperties> func)
        {
            var message = Services.GetService<MessageService>();

            return message.SendAsync(Context, func);
        }
    }
}
