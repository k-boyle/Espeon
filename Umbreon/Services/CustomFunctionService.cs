using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.Contexts;
using Umbreon.Commands.Preconditions;
using Umbreon.Core;
using Umbreon.Core.Entities.Guild;

namespace Umbreon.Services
{
    [Service]
    public class CustomFunctionService
    {
        private readonly DatabaseService _database;
        private readonly CommandService _commandService;
        private readonly EvalService _eval;
        private readonly LogService _logs;
        private const string Name = "Created Functions";
        private ModuleInfo _module;

        public CustomFunctionService(DatabaseService database, CommandService commandService, EvalService eval, LogService logs)
        {
            _database = database;
            _commandService = commandService;
            _eval = eval;
            _logs = logs;
        }

        public async Task LoadFuncsAsync(BaseSocketClient client)
        {
            await AddFuncsAsync(client);
            _logs.NewLogEvent(LogSeverity.Info, LogSource.CustomFuncs, "Custom functions have been loaded");
        }

        private async Task AddFuncsAsync(BaseSocketClient client)
        {
            if (!(_module is null))
                await _commandService.RemoveModuleAsync(_module);

            var allFunctions = client.Guilds.Select(x => _database.TempLoad<GuildObject>("guilds", x.Id))
                .SelectMany(y => y.CustomFunctions);

            _module = await _commandService.CreateModuleAsync("", module =>
            {
                module.WithSummary("Custom functions for the bot");
                module.WithName(Name);

                foreach (var func in allFunctions)
                {
                    module.AddCommand(func.FunctionName, FunctionCallbackAsync, function =>
                    {
                        function.WithName(func.FunctionName);
                        function.WithSummary(func.Summary);
                        function.AddAttributes(new UsageAttribute(func.FunctionName));

                        if (func.IsPrivate)
                            function.AddPrecondition(new RequireOwnerAttribute());

                        if (func.GuildId != 0)
                            function.AddPrecondition(new RequireGuildAttribute(func.GuildId));
                    });
                }
            });
        }

        private async Task FunctionCallbackAsync(ICommandContext context, object[] _, IServiceProvider services, CommandInfo info)
        {
            var client = context.Client as BaseSocketClient;
            var allFuncs = client?.Guilds.Select(x => _database.GetObject<GuildObject>("guilds", x.Id))
                .SelectMany(y => y.CustomFunctions);
            var found = allFuncs?.FirstOrDefault(x => string.Equals(x.FunctionName, info.Name, StringComparison.CurrentCultureIgnoreCase));
            await _eval.EvaluateAsync(context as UmbreonContext, found?.FunctionCallback, false, services);
        }

        public async Task NewFuncAsync(ICommandContext context, CustomFunction function)
        {
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.CustomFunctions.Add(function);
            _database.UpdateObject("guilds", guild);
            await LoadFuncsAsync(context.Client as BaseSocketClient);
        }

        public async Task RemoveFuncAsync(ICommandContext context, CustomFunction function)
        {
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.CustomFunctions.Remove(function);
            _database.UpdateObject("guilds", guild);
            await LoadFuncsAsync(context.Client as BaseSocketClient);
        }

        public async Task UpdateFunctionAsync(ICommandContext context, CustomFunction before, CustomFunction after)
        {
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.CustomFunctions[guild.CustomFunctions.IndexOf(before)] = after;
            _database.UpdateObject("guilds", guild);
            await LoadFuncsAsync(context.Client as BaseSocketClient);
        }

        public IEnumerable<CustomFunction> GetFuncs(ICommandContext context)
            => GetFuncs(context.Guild.Id);

        private IEnumerable<CustomFunction> GetFuncs(ulong guildId)
            => _database.GetObject<GuildObject>("guilds", guildId).CustomFunctions;

        private string RemoveGroupName(string inStr)
        {
            var groups = _commandService.Modules.Where(x => !(x.Group is null)).Select(y => y.Group);
            var outStr = inStr;
            foreach (var group in groups)
            {
                if (inStr.Contains(group))
                {
                    outStr = inStr.Replace($"{group} ", "");
                }
            }
            return outStr;
        }

        public bool IsReserved(string toCheck)
        {
            var cmds = _commandService.Commands.SelectMany(x => x.Aliases).ToList();
            var reserved = new List<string>();
            foreach (var cmd in cmds)
            {
                reserved.Add(RemoveGroupName(cmd));
            }

            return reserved.Contains(toCheck, StringComparer.CurrentCultureIgnoreCase);
        }
    }
}
