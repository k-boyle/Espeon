using Discord;
using Discord.Commands;
using System.Diagnostics;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;

namespace Umbreon.Modules
{
    [Name("Misc Commands")]
    [Summary("Commands that don't fit into a category")]
    public class MiscCommands : UmbreonBase<GuildCommandContext>
    {
        // TODO reminders, admin/mod list

        [Command("ping")]
        [Name("Ping")]
        [Summary("Get the response time of the bot")]
        [Usage("ping")]
        public async Task Ping()
        {
            var sw = new Stopwatch();
            sw.Start();
            var msg = await SendMessageAsync("Ping: ");
            sw.Stop();
            await (msg as IUserMessage).ModifyAsync(x => x.Content = $"Ping: {sw.ElapsedMilliseconds}ms\nLatency: {Context.Client.Latency}ms");
        }

        [Command("c")]
        [Name("Clear Responses")]
        [Usage("c")]
        [Summary("Will clear all responses from the bot to you in the last 5 minutes")]
        public async Task Clear()
        {
            await Message.ClearMessages(Context);
        }

    }
}
