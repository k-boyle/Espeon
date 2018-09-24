using Discord;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Espeon.Commands.Contexts;
using Espeon.Services;

namespace Espeon.Core.Entities
{
    public class Globals
    {
        public EspeonContext Context { get; set; }
        public IServiceProvider Services { get; set; }
        public MessageService Message { get; set; }
        public HttpClient HttpClient { get; set; }

        public Task<IUserMessage> SendMessageAsync(string content, bool isTTS = false, Embed embed = null)
            => Message.NewMessageAsync(Context, content, isTTS, embed);
    }
}
