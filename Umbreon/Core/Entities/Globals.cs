using System;
using System.Threading.Tasks;
using Discord;
using Umbreon.Modules.Contexts;
using Umbreon.Services;

namespace Umbreon.Core.Entities
{
    public class Globals
    {
        public GuildCommandContext Context { get; set; }
        public IServiceProvider Services { get; set; }
        public MessageService Message { get; set; }

        public Task<IUserMessage> SendMessageAsync(string content, bool isTTS = false, Embed embed = null)
            => Message.NewMessageAsync(Context, content, isTTS, embed);
    }
}
