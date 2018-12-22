using Discord;
using Espeon.Core.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Bases
{
    public abstract class EspeonBase : ModuleBase<IEspeonContext>
    {
        public IMessageService Message { get; set; }
        public IResponseService Response { get; set; }
        public IDatabaseService Database { get; set; }

        public Module Module { get; private set; }
        public Command Command { get; private set; }

        public string ResponsePack { get; private set; }

        protected Task<IUserMessage> SendMessageAsync(Embed embed)
        {
            return SendMessageAsync(string.Empty, embed);
        }

        protected Task<IUserMessage> SendMessageAsync(string content, Embed embed = null)
        {
            return Message.SendMessageAsync(Context, content, embed);
        }

        protected override async Task BeforeExecutedAsync(Command command)
        {
            Module = command.Module;
            Command = command;

            var pack = await Response.GetUsersPackAsync(Context.User.Id);
            ResponsePack = pack;
        }
    }
}
