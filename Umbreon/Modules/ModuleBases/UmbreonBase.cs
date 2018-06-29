using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Interactive.Interfaces;
using Discord.Commands;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public class UmbreonBase<T> : InteractiveBase<T> where T : class, ICommandContext
    {
        public MessageService Message { get; set; }

        public async Task<IMessage> SendMessageAsync(string content, Embed embed = null, IPaginatedMessage paginator = null)
        {
            return await Message.SendMessageAsync(Context, content, embed, paginator);
        }
    }
}
