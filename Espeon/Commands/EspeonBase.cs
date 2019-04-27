using Discord;
using Discord.WebSocket;
using Espeon.Services;
using Qmmands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public abstract class EspeonBase : ModuleBase<EspeonContext>, IDisposable
    {
        public MessageService Message { get; set; }
        public InteractiveService Interactive { get; set; }
        public ResponseService Responses { get; set; }
        public IServiceProvider Services { get; set; }

        protected Task<IUserMessage> SendMessageAsync(Embed embed)
        {
            return SendMessageAsync(string.Empty, embed);
        }

        protected Task<IUserMessage> SendMessageAsync(string content, Embed embed = null)
        {
            return Message.SendAsync(Context, x =>
            {
                x.Content = content;
                x.Embed = embed;
            });
        }

        protected Task<IUserMessage> SendFileAsync(Stream stream, string fileName, string content = null, 
            Embed embed = null)
        {
            return Message.SendAsync(Context, x =>
            {
                x.Content = content;
                x.Embed = embed;
                x.Stream = stream;
                x.FileName = fileName;
            });
        }

        protected async Task<IUserMessage> SendOkAsync(int index, params object[] args)
        {
            var cmd = Context.Command;
            var user = Context.Invoker;

            var resp = Responses.GetResponse(cmd.Module.Name, cmd.Name, user.ResponsePack, index, args);

            var response = ResponseBuilder.Message(Context, resp);
            return await SendMessageAsync(response);
        }

        protected async Task<IUserMessage> SendNotOkAsync(int index, params object[] args)
        {
            var cmd = Context.Command;
            var user = Context.Invoker;

            var resp = Responses.GetResponse(cmd.Module.Name, cmd.Name, user.ResponsePack, index, args);

            var response = ResponseBuilder.Message(Context, resp, false);
            return await SendMessageAsync(response);
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

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
