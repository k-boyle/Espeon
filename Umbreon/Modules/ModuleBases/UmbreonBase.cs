using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Umbreon.Interactive;
using Umbreon.Paginators;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public class UmbreonBase<T> : InteractiveBase<T> where T : class, ICommandContext
    {
        public MessageService Message { get; set; }
        public IServiceProvider Services { get; set; }

        public Task<IUserMessage> SendMessageAsync(string content, bool isTTS = false, Embed embed = null)
            => Message.SendMessageAsync(Context, content, isTTS, embed);

        public Task<IUserMessage> SendPaginatedMessageAsync(BasePaginator paginator)
            => Message.SendPaginatedMessageAsync(Context, paginator);

        public Task<int> ClearMessages(int amount)
            => Message.ClearMessages(Context, amount);

        public Task DeleteMessageAsync(IUserMessage message)
            => Message.DeleteMessageAsync(Context, message);
    }
}
