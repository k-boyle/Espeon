using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Threading;
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

        [Command("ping", RunMode = RunMode.Async)]
        [Name("Ping")]
        [Summary("Get the response time of the bot")]
        [Usage("ping")]
        public async Task Ping()
        {
            ulong target = 0;
            var cts = new CancellationTokenSource();

            Task WaitTarget(SocketMessage message)
            {
                if (message.Id != target) return Task.CompletedTask;
                cts.Cancel();
                return Task.CompletedTask;
            }

            var latency = Context.Client.Latency;
            var sw = Stopwatch.StartNew();
            var msg = await SendMessageAsync($"heartbeat: {latency}ms, init: ---, rtt: ---");
            var init = sw.ElapsedMilliseconds;
            target = msg.Id;
            sw.Restart();
            Context.Client.MessageReceived += WaitTarget;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cts.Token);
            }
            catch (TaskCanceledException)
            {
                var rtt = sw.ElapsedMilliseconds;
                sw.Stop();
                await msg.ModifyAsync(x => x.Content = $"heartbeat: {latency}ms, init: {init}ms, rtt: {rtt}ms");
                return;
            }
            finally
            {
                Context.Client.MessageReceived -= WaitTarget;
            }
            sw.Stop();
            await msg.ModifyAsync(x => x.Content = $"heartbeat: {latency}ms, init: {init}ms, rtt: timeout");
        }

        [Command("c", RunMode = RunMode.Async)]
        [Name("Clear Responses")]
        [Usage("c")]
        [Summary("Will clear all responses from the bot to you in the last 5 minutes")]
        public async Task Clear()
        {
            await SendMessageAsync("Clearing messages");
            await Task.Delay(1000);
            await Message.ClearMessages(Context);
        }

    }
}
