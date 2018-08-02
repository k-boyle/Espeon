using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Models.Database;
using Umbreon.Preconditions;

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

        public CustomFunctionService(DatabaseService database, CommandService commandService, EvalService eval, LogService logs)
        {
            _database = database;
            _commandService = commandService;
            _eval = eval;
            _logs = logs;
        }

        public async Task LoadFuncs(BaseSocketClient client)
        {
            await AddFuncs(client);
            _logs.NewLogEvent(LogSeverity.Info, LogSource.CustomFuncs, "Custom functions have been loaded");
        }

        private async Task AddFuncs(BaseSocketClient client)
        {
            var allFunctions = client.Guilds.Select(x => _database.GetGuild(x.Id))
                .SelectMany(y => y.CustomFunctions);

            await _commandService.CreateModuleAsync(Name, module =>
            {
                module.WithSummary("Custom functions for the bot");
                module.AddAliases("");

                foreach (var func in allFunctions)
                {
                    module.AddCommand(func.FunctionName, FunctionCallback, function =>
                    {
                        function.WithName(func.FunctionName);
                        function.WithSummary(func.Summary);
                        function.AddAttributes(new UsageAttribute(func.FunctionName));

                        if (func.IsPrivate)
                            function.AddPrecondition(new RequireOwnerAttribute());

                        if (func.GuildId != 0)
                            function.AddPrecondition(new RequireGuild(func.GuildId));
                    });
                }
            });
        }

        private async Task FunctionCallback(ICommandContext context, object[] _, IServiceProvider services, CommandInfo info)
        {
            var client = context.Client as BaseSocketClient;
            var allFuncs = client?.Guilds.Select(x => _database.GetGuild(x.Id))
                .SelectMany(y => y.CustomFunctions);
            var found = allFuncs?.FirstOrDefault(x => string.Equals(x.FunctionName, info.Name, StringComparison.CurrentCultureIgnoreCase));
            await _eval.EvaluateAsync(context, found?.FunctionCallback, false, services);
        }

        public async Task NewFunc(ICommandContext context, CustomFunction function)
        {
            var guild = _database.GetGuild(context);
            guild.CustomFunctions.Add(function);
            _database.UpdateGuild(guild);
            await UpdateFuncs(context.Client as BaseSocketClient);
        }

        public async Task RemoveFunc(ICommandContext context, CustomFunction function)
        {
            var guild = _database.GetGuild(context);
            guild.CustomFunctions.Remove(function);
            _database.UpdateGuild(guild);
            await UpdateFuncs(context.Client as BaseSocketClient);
        }

        public async Task UpdateFunction(ICommandContext context, CustomFunction before, CustomFunction after)
        {
            var guild = _database.GetGuild(context);
            guild.CustomFunctions[guild.CustomFunctions.IndexOf(before)] = after;
            _database.UpdateGuild(guild);
            await UpdateFuncs(context.Client as BaseSocketClient);
        }

        private async Task UpdateFuncs(BaseSocketClient client)
        {
            await _commandService.RemoveModuleAsync(
                _commandService.Modules.FirstOrDefault(x => x.Name == Name));
            await LoadFuncs(client);
            _logs.NewLogEvent(LogSeverity.Info, LogSource.CustomFuncs, "Custom functions have been updated");
        }

        public IEnumerable<CustomFunction> GetFuncs(ICommandContext context)
            => GetFuncs(context.Guild.Id);

        private IEnumerable<CustomFunction> GetFuncs(ulong guildId)
            => _database.GetGuild(guildId).CustomFunctions;

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
