using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Umbreon.Interactive;
using Umbreon.Modules.Contexts;
using Umbreon.Paginators;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public abstract class UmbreonBase : UmbreonBase<UmbreonContext>
    {
        protected Stream Stream = null;

        protected async Task<JObject> SendRequest(string url)
        {
            var client = Context.HttpClient;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var response = await client.GetAsync(url))
            {
                return JObject.Parse(await response.Content.ReadAsStringAsync());
            }
        }

        protected async Task<Stream> GetStream(string url)
        {
            var client = Context.HttpClient;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetStreamAsync(url);
            Stream = response;
            return Stream;
        }
    }

    public abstract class UmbreonBase<T> : ModuleBase<T> where T : class, ICommandContext
    {
        public MessageService Message { get; set; }
        public IServiceProvider Services { get; set; }
        public InteractiveService Interactive { get; set; }

        protected Task<IUserMessage> SendMessageAsync(string content, bool isTTS = false, Embed embed = null)
            => Message.SendMessageAsync(Context, content, isTTS, embed);

        protected Task<IUserMessage> SendPaginatedMessageAsync(BasePaginator paginator)
            => Message.SendPaginatedMessageAsync(Context, paginator);

        protected Task<int> ClearMessages(int amount)
            => Message.ClearMessagesAsync(Context, amount);

        protected Task DeleteMessageAsync(IUserMessage message)
            => Message.DeleteMessageAsync(Context, message);

        protected Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null)
            => Interactive.NextMessageAsync(Context, fromSourceUser, inSourceChannel, timeout);
    }
}
