using System;
using Discord;
using Discord.Commands;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
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
            ulong target = 0;
            CancellationTokenSource cts = new CancellationTokenSource();

            Task WaitTarget(SocketMessage message)
            {
                if (message.Id != target) return Task.CompletedTask;
                cts.Cancel();
                return Task.CompletedTask;
            }

            var latency = Context.Client.Latency;
            var s = Stopwatch.StartNew();
            var m = await SendMessageAsync($"heartbeat: {latency}ms, init: ---, rtt: ---");
            var init = s.ElapsedMilliseconds;
            target = m.Id;
            s.Restart();
            Context.Client.MessageReceived += WaitTarget;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cts.Token);
            }
            catch (TaskCanceledException)
            {
                var rtt = s.ElapsedMilliseconds;
                s.Stop();
                await m.ModifyAsync(x => x.Content = $"heartbeat: {latency}ms, init: {init}ms, rtt: {rtt}ms");
                return;
            }
            finally
            {
                Context.Client.MessageReceived -= WaitTarget;
            }
            s.Stop();
            await m.ModifyAsync(x => x.Content = $"heartbeat: {latency}ms, init: {init}ms, rtt: timeout");
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
