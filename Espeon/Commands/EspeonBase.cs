using Discord;
using Discord.WebSocket;
using Espeon.Interactive;
using Espeon.Interactive.Criteria;
using Espeon.Interactive.Paginator;
using Espeon.Services;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public abstract class EspeonBase : ModuleBase<EspeonContext>
    {
        public MessageService Message { get; set; }
        public InteractiveService Interactive { get; set; }
        public IServiceProvider Services { get; set; }   
        
        [DoNotAutomaticallyInject]
        public Module Module { get; set; }

        [DoNotAutomaticallyInject]
        public Command Command { get; set; }

        protected Task<IUserMessage> SendMessageAsync(Embed embed)
        {
            return SendMessageAsync(string.Empty, embed);
        }

        protected Task<IUserMessage> SendMessageAsync(string content, Embed embed = null)
        {
            return Message.SendMessageAsync(Context, x =>
            {
                x.Content = content;
                x.Embed = embed;
            });
        }

        protected Task<IUserMessage> SendFileAsync(Stream stream, string fileName, string content = null, 
            Embed embed = null)
        {
            return Message.SendMessageAsync(Context, x =>
            {
                x.Content = content;
                x.Embed = embed;
                x.Stream = stream;
                x.FileName = fileName;
            });
        }

        protected async Task<IUserMessage> SendOkAsync(int index, params object[] args)
        {
            var module = await Context.CommandStore.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == Module.Name);

            var command = module.Commands.FirstOrDefault(x => x.Name == Command.Name);

            var user = await Context.UserStore.GetOrCreateUserAsync(Context.User);

            var responses = command.Responses[user.ResponsePack];

            var response = ResponseBuilder.Message(Context, string.Format(responses[index], args));
            return await SendMessageAsync(response);
        }

        protected async Task<IUserMessage> SendNotOkAsync(int index, params object[] args)
        {
            var module = await Context.CommandStore.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == Module.Name);

            var command = module.Commands.FirstOrDefault(x => x.Name == Command.Name);

            var user = await Context.UserStore.GetOrCreateUserAsync(Context.User);

            var responses = command.Responses[user.ResponsePack];

            var response = ResponseBuilder.Message(Context, string.Format(responses[index], args), false);
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

        protected override Task BeforeExecutedAsync(Command command)
        {
            Module = command.Module;
            Command = command;

            return Task.CompletedTask;
        }
    }
}
