using Discord;
using Espeon.Core.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Bases
{
    public abstract class EspeonBase : ModuleBase<IEspeonContext>
    {
        public IMessageService Message { get; set; }

        protected Task<IUserMessage> SendMessageAsync(string content, Embed embed = null)
        {
            return Message.SendMessageAsync(Context, content, embed);
        }
    }
}
