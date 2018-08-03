using Discord.Commands;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Extensions;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Paginators.CommandMenu;

namespace Umbreon.Modules
{
    [Name("Misc Commands")]
    [Summary("Commands that don't fit into a category")]
    public class MiscCommands : UmbreonBase<GuildCommandContext>
    {
        // TODO reminders, admin/mod list

        private readonly CommandService _commands;

        public MiscCommands(CommandService commands)
        {
            _commands = commands;
        }

        [Command("ping", RunMode = RunMode.Async)]
        [Name("Ping")]
        [Summary("Get the response time of the bot")]
        [Usage("ping")]
        public async Task Ping()
        {
            var sw = new Stopwatch();
            sw.Start();
            var reply = await SendMessageAsync($"Latency: {Context.Client.Latency}. Ping: ");
            sw.Stop();
            await reply.ModifyAsync(x => x.Content = $"Latency: {Context.Client.Latency}ms Ping: {sw.ElapsedMilliseconds}ms");
        }

        [Command("c", RunMode = RunMode.Async)]
        [Name("Clear Responses")]
        [Usage("c")]
        [Summary("Will clear all responses from the bot to you in the last 5 minutes")]
        public async Task Clear(int amount = 5)
        {
            var m = await SendMessageAsync("Clearing messages");
            await ClearMessages(amount);
            await Task.Delay(1000);
            await DeleteMessageAsync(m);
        }

        [Command("Menu")]
        [Name("Command Menu")]
        [Usage("menu")]
        [Summary("Displays a menu where you can navigate and execute commands from")]
        public async Task Menu()
        {
            var dict = new Dictionary<ModuleInfo, IEnumerable<CommandInfo>>();
            foreach (var module in _commands.Modules)
            {
                if(!(await module.CheckPermissionsAsync(Context, Services)).IsSuccess) continue;
                var cmdList = new List<CommandInfo>();
                foreach (var cmd in module.Commands)
                {
                    if(!(await cmd.CheckPreconditionsAsync(Context, Services)).IsSuccess) continue;
                    cmdList.Add(cmd);
                }
                dict.Add(module, cmdList);
            }

            await SendPaginatedMessageAsync(new CommandMenuMessage(dict));
        }
    }
}
