using Discord;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Umbreon.Commands.Contexts;
using Umbreon.Services;

namespace Umbreon.Core.Entities
{
    public class Globals
    {
        public UmbreonContext Context { get; set; }
        public IServiceProvider Services { get; set; }
        public MessageService Message { get; set; }
        public HttpClient HttpClient { get; set; }

        public Task<IUserMessage> SendMessageAsync(string content, bool isTTS = false, Embed embed = null)
            => Message.NewMessageAsync(Context, content, isTTS, embed);
    }
}
