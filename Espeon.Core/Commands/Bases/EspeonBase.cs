using System.Threading.Tasks;
using Discord;
using Espeon.Core.Services;
using Qmmands;

namespace Espeon.Core.Commands.Bases
{
    public abstract class EspeonBase : ModuleBase<IEspeonContext>
    {
        public IMessageService Message { get; }

        protected Task<IUserMessage> SendMessageAsync(string content, Embed embed = null)
        {
            return Message.SendMessageAsync(Context, content, embed);
        }
    }
}
