using Discord.Rest;
using Espeon.Commands;
using Espeon.Entities;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface IMessageService
    {
        Task<RestUserMessage> SendAsync(EspeonContext context, Action<NewMessageProperties> properties);
        Task DeleteMessagesAsync(EspeonContext context, int amount);
    }
}
