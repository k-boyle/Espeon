﻿using Discord;
using Discord.WebSocket;
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
            var module = await Context.CommandStore.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == cmd.Module.Name);

            var command = module.Commands.FirstOrDefault(x => x.Name == cmd.Name);

            var user = await Context.GetInvokerAsync();

            var responses = Responses.GetResponses(cmd.Module.Name, cmd.Name);

            var response = ResponseBuilder.Message(Context, string.Format(responses[user.ResponsePack][index], args));
            return await SendMessageAsync(response);
        }

        protected async Task<IUserMessage> SendNotOkAsync(int index, params object[] args)
        {
            var cmd = Context.Command;
            var module = await Context.CommandStore.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == Context.Command.Module.Name);

            var command = module.Commands.FirstOrDefault(x => x.Name == cmd.Name);

            var user = await Context.GetInvokerAsync();
            
            var responses = Responses.GetResponses(cmd.Module.Name, cmd.Name);

            var response = ResponseBuilder.Message(Context, 
                string.Format(responses[user.ResponsePack][index], args), false);
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
    }
}
