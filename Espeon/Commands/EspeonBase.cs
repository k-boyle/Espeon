using Discord;
using Discord.WebSocket;
using Espeon.Interactive;
using Espeon.Interactive.Criteria;
using Espeon.Interactive.Paginator;
using Espeon.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public abstract class EspeonBase : ModuleBase<EspeonContext>
    {
        public MessageService Message { get; set; }
        public ResponseService Response { get; set; }
        public InteractiveService Interactive { get; set; }
        public IServiceProvider Services { get; set; }

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

        protected Task<IUserMessage> SendOkAsync(string content)
        {
            var response = ResponseBuilder.Message(Context, content);
            return SendMessageAsync(response);
        }

        protected Task<IUserMessage> SendNotOkAsync(string content)
        {
            var response = ResponseBuilder.Message(Context, content, false);
            return SendMessageAsync(response);
        }

        protected Task<SocketUserMessage> NextMessageAsync(ICriterion<SocketUserMessage> criterion,
            TimeSpan? timeout = null)
        {
            return Interactive.NextMessageAsync(Context, criterion, timeout);
        }

        protected Task<bool> TryAddCallbackAsync(IReactionCallback callback, TimeSpan? timeout = null)
        {
            return Interactive.TryAddCallbackAsync(callback, timeout);
        }

        protected Task SendPaginatedMessageAsync(PaginatorBase paginator, TimeSpan? timeout = null)
        {
            return Interactive.SendPaginatedMessageAsync(paginator, timeout);
        }

        protected override async Task BeforeExecutedAsync(Command command)
        {
            Module = command.Module;
            Command = command;

            var pack = await Response.GetUsersPackAsync(Context);
            ResponsePack = pack;
        }
    }
}
